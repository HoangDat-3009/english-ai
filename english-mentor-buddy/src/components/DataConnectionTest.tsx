import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useLeaderboard, useRecentActivities, useUserStats } from '@/hooks/useStats';
import { getSynchronizedData } from '@/services/statsService';
import { AlertCircle, ArrowRight, CheckCircle } from 'lucide-react';
import React from 'react';

const DataConnectionTest: React.FC = () => {
  // Fetch data from hooks (same as used in Progress and Leaderboard pages)
  const { data: userStats, isLoading: statsLoading } = useUserStats(1);
  const { data: leaderboard, isLoading: leaderboardLoading } = useLeaderboard(10, undefined, "all");
  const { data: activities, isLoading: activitiesLoading } = useRecentActivities(1, 5);
  const syncData = getSynchronizedData();

  // Test data consistency
  const currentUser = leaderboard?.find(user => user.username === "englishlearner01");
  const progressScore = userStats?.averageScore || syncData.userStats.averageScore;
  const leaderboardScore = currentUser?.totalScore || 0;

  const isDataConnected = Math.abs(progressScore - leaderboardScore) <= 50; // Allow small variance

  return (
    <div className="container mx-auto p-6 space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            {isDataConnected ? (
              <CheckCircle className="w-6 h-6 text-green-500" />
            ) : (
              <AlertCircle className="w-6 h-6 text-red-500" />
            )}
            Data Connection Test
          </CardTitle>
          <CardDescription>
            Kiểm tra tính liên kết giữa dữ liệu Progress và Leaderboard
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Progress Page Data */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Progress Page Data</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex justify-between items-center">
                  <span>User Score:</span>
                  <Badge variant="outline">
                    {statsLoading ? "Loading..." : `${progressScore} points`}
                  </Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span>Total Exercises:</span>
                  <Badge variant="outline">
                    {statsLoading ? "Loading..." : syncData.userStats.totalExercises}
                  </Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span>Recent Activities:</span>
                  <Badge variant="outline">
                    {activitiesLoading ? "Loading..." : `${activities?.length || 0} items`}
                  </Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span>Current Streak:</span>
                  <Badge variant="outline">
                    {statsLoading ? "Loading..." : `${syncData.userStats.currentStreak} days`}
                  </Badge>
                </div>
              </CardContent>
            </Card>

            {/* Leaderboard Data */}
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Leaderboard Data</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <div className="flex justify-between items-center">
                  <span>User Score:</span>
                  <Badge variant="outline">
                    {leaderboardLoading ? "Loading..." : `${leaderboardScore} points`}
                  </Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span>User Rank:</span>
                  <Badge variant="outline">
                    {leaderboardLoading ? "Loading..." : `#${currentUser?.rank || "N/A"}`}
                  </Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span>Total Users:</span>
                  <Badge variant="outline">
                    {leaderboardLoading ? "Loading..." : `${leaderboard?.length || 0} users`}
                  </Badge>
                </div>
                <div className="flex justify-between items-center">
                  <span>Last Update:</span>
                  <Badge variant="outline">
                    {leaderboardLoading ? "Loading..." : new Date(currentUser?.lastUpdate || "").toLocaleDateString()}
                  </Badge>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Connection Status */}
          <div className="mt-6 p-4 rounded-lg border">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <span className="font-medium">Data Connection Status:</span>
                {isDataConnected ? (
                  <Badge variant="default" className="bg-green-500">Connected</Badge>
                ) : (
                  <Badge variant="destructive">Disconnected</Badge>
                )}
              </div>
              <ArrowRight className="w-5 h-5 text-muted-foreground" />
            </div>
            
            <div className="mt-3 text-sm text-muted-foreground">
              <div className="flex justify-between">
                <span>Progress Score:</span>
                <span>{progressScore}</span>
              </div>
              <div className="flex justify-between">
                <span>Leaderboard Score:</span>
                <span>{leaderboardScore}</span>
              </div>
              <div className="flex justify-between">
                <span>Score Difference:</span>
                <span>{Math.abs(progressScore - leaderboardScore)}</span>
              </div>
              
              {isDataConnected ? (
                <p className="mt-2 text-green-600">
                  ✅ Data is synchronized between Progress and Leaderboard pages!
                </p>
              ) : (
                <p className="mt-2 text-red-600">
                  ❌ Data mismatch detected. Scores differ by more than 50 points.
                </p>
              )}
            </div>
          </div>

          {/* Recent Activities Preview */}
          <div className="mt-6">
            <h4 className="font-medium mb-3">Recent Activities (Used in Progress)</h4>
            <div className="space-y-2">
              {(activities || syncData.activities.slice(0, 3)).map((activity) => (
                <div key={activity.id} className="flex justify-between items-center p-2 bg-muted rounded">
                  <span className="text-sm">{activity.type} - {activity.topic}</span>
                  <Badge variant="outline">{activity.score}%</Badge>
                </div>
              ))}
            </div>
          </div>

          {/* Leaderboard Preview */}
          <div className="mt-6">
            <h4 className="font-medium mb-3">Top Users (Used in Leaderboard)</h4>
            <div className="space-y-2">
              {(leaderboard?.slice(0, 3) || syncData.leaderboard.slice(0, 3)).map((user) => (
                <div key={user.userId} className="flex justify-between items-center p-2 bg-muted rounded">
                  <span className="text-sm">#{user.rank} {user.username}</span>
                  <Badge variant="outline">{user.totalScore} pts</Badge>
                </div>
              ))}
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default DataConnectionTest;