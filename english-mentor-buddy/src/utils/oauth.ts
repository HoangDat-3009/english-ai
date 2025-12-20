import { authService } from '@/services/authService';

/**
 * Initialize and handle Google OAuth login
 */
export const loginWithGoogle = async (): Promise<void> => {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
  
  if (!clientId) {
    throw new Error('Google Client ID is not configured. Please set VITE_GOOGLE_CLIENT_ID in .env file');
  }

  // Load Google Identity Services script
  await loadGoogleScript();

  return new Promise((resolve, reject) => {
    // Add timeout to prevent hanging
    const timeoutId = setTimeout(() => {
      reject(new Error('Google login timeout'));
    }, 60000); // 60 seconds timeout

    // Show Google sign-in popup using OAuth2 token client
    const tokenClient = window.google.accounts.oauth2.initTokenClient({
      client_id: clientId,
      scope: 'email profile',
      callback: async (tokenResponse: any) => {
        clearTimeout(timeoutId);
        
        try {
          if (tokenResponse.error) {
            reject(new Error(tokenResponse.error || 'Google OAuth error'));
            return;
          }

          // Fetch user info using access token
          const userInfoResponse = await fetch('https://www.googleapis.com/oauth2/v2/userinfo', {
            headers: {
              Authorization: `Bearer ${tokenResponse.access_token}`,
            },
          });

          if (!userInfoResponse.ok) {
            const errorText = await userInfoResponse.text();
            throw new Error(`Failed to fetch user info: ${errorText}`);
          }

          const userInfo = await userInfoResponse.json();
          
          // Validate required fields
          if (!userInfo.id) {
            reject(new Error('Google user ID is missing'));
            return;
          }

          if (!userInfo.email) {
            reject(new Error('Google email is required but not provided. Please ensure your Google account has a verified email address.'));
            return;
          }
          
          const result = await authService.oauthLogin({
            provider: 'google',
            providerId: userInfo.id,
            email: userInfo.email,
            fullName: userInfo.name || undefined,
            avatar: userInfo.picture || undefined,
          });

          if (result.success && result.user) {
            // User is already saved to localStorage by authService.oauthLogin
            resolve();
          } else {
            reject(new Error(result.message || 'Google login failed'));
          }
        } catch (error: any) {
          console.error('Google OAuth error:', error);
          reject(error instanceof Error ? error : new Error('Google OAuth error'));
        }
      },
    });

    try {
      tokenClient.requestAccessToken();
    } catch (error: any) {
      clearTimeout(timeoutId);
      reject(error instanceof Error ? error : new Error('Failed to request Google access token'));
    }
  });
};

/**
 * Load Google Identity Services script
 */
const loadGoogleScript = (): Promise<void> => {
  return new Promise((resolve, reject) => {
    if (window.google?.accounts) {
      resolve();
      return;
    }

    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error('Failed to load Google Identity Services'));
    document.head.appendChild(script);
  });
};

/**
 * Initialize and handle Facebook OAuth login
 */
export const loginWithFacebook = async (): Promise<void> => {
  const appId = import.meta.env.VITE_FACEBOOK_APP_ID;
  
  if (!appId) {
    throw new Error('Facebook App ID is not configured. Please set VITE_FACEBOOK_APP_ID in .env file');
  }

  // Load Facebook SDK
  await loadFacebookScript(appId);

  return new Promise((resolve, reject) => {
    // Add timeout to prevent hanging
    const timeoutId = setTimeout(() => {
      reject(new Error('Facebook login timeout'));
    }, 60000); // 60 seconds timeout

    window.FB.login(
      async (response: any) => {
        clearTimeout(timeoutId);
        
        if (response.authResponse) {
          try {
            // Fetch user info with error handling
            window.FB.api('/me', { fields: 'id,name,email,picture' }, async (userInfo: any) => {
              try {
                // Check for API errors
                if (userInfo.error) {
                  const errorMsg = userInfo.error.message || userInfo.error.error_msg || 'Failed to fetch Facebook user info';
                  reject(new Error(errorMsg));
                  return;
                }

                // Validate required fields
                if (!userInfo.id) {
                  reject(new Error('Facebook user ID is missing'));
                  return;
                }

                if (!userInfo.email) {
                  reject(new Error('Facebook email is required but not provided. Please ensure your Facebook account has a verified email address.'));
                  return;
                }

                const result = await authService.oauthLogin({
                  provider: 'facebook',
                  providerId: userInfo.id,
                  email: userInfo.email,
                  fullName: userInfo.name || undefined,
                  avatar: userInfo.picture?.data?.url || undefined,
                });

                if (result.success && result.user) {
                  // User is already saved to localStorage by authService.oauthLogin
                  resolve();
                } else {
                  reject(new Error(result.message || 'Facebook login failed'));
                }
              } catch (error: any) {
                console.error('Facebook OAuth error:', error);
                reject(error instanceof Error ? error : new Error('Facebook OAuth error'));
              }
            });
          } catch (error: any) {
            console.error('Facebook API error:', error);
            reject(error instanceof Error ? error : new Error('Facebook API error'));
          }
        } else {
          // User cancelled or login failed
          if (response.status === 'not_authorized') {
            reject(new Error('Facebook login was not authorized'));
          } else {
            reject(new Error('Facebook login was cancelled or failed'));
          }
        }
      },
      { scope: 'email,public_profile' }
    );
  });
};

/**
 * Load Facebook SDK script
 */
const loadFacebookScript = (appId: string): Promise<void> => {
  return new Promise((resolve, reject) => {
    // Check if already loaded
    if (window.FB) {
      resolve();
      return;
    }

    // Set up initialization callback
    window.fbAsyncInit = function() {
      try {
        window.FB.init({
          appId: appId,
          cookie: true,
          xfbml: false,
          version: 'v18.0',
        });
        resolve();
      } catch (error) {
        reject(new Error('Failed to initialize Facebook SDK'));
      }
    };

    // Load script
    const script = document.createElement('script');
    script.src = 'https://connect.facebook.net/en_US/sdk.js';
    script.async = true;
    script.defer = true;
    script.onerror = () => reject(new Error('Failed to load Facebook SDK'));
    document.head.appendChild(script);

    // Timeout after 10 seconds
    setTimeout(() => {
      if (!window.FB) {
        reject(new Error('Facebook SDK loading timeout'));
      }
    }, 10000);
  });
};

// Type declarations
declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: any) => void;
        };
        oauth2: {
          initTokenClient: (config: any) => {
            requestAccessToken: () => void;
          };
        };
      };
    };
    FB?: {
      init: (config: any) => void;
      login: (callback: (response: any) => void, options?: any) => void;
      api: (path: string, params: any, callback: (response: any) => void) => void;
    };
    fbAsyncInit?: () => void;
  }
}

