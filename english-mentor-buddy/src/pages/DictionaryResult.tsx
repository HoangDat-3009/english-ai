import React, { useState, useEffect } from 'react';
import { ArrowLeft, Volume2 } from 'lucide-react';
import Header from '@/components/Navbar';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { useNavigate, useLocation } from 'react-router-dom';
import { dictionaryService } from '@/services/dictionaryService';
import { useToast } from '@/hooks/use-toast';

const DictionaryResult: React.FC = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { toast } = useToast();
    const searchParams = new URLSearchParams(location.search);
    const keyword = searchParams.get('keyword') || '';

    const [wordData, setWordData] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    // Function to play pronunciation
    const playPronunciation = () => {
        // This would typically use the Web Speech API or an external pronunciation service
        if (keyword) {
            const utterance = new SpeechSynthesisUtterance(keyword);
            utterance.lang = 'en-US';
            window.speechSynthesis.speak(utterance);
        }
    };

    useEffect(() => {
        const fetchWordDefinition = async () => {
            if (!keyword) {
                navigate('/dictionary');
                return;
            }

            setIsLoading(true);

            try {
                console.log('Fetching word definition for:', keyword);
                const result = await dictionaryService.searchWord(keyword);

                if (result) {
                    // Treat the response as a string
                    setWordData(typeof result === 'string' ? result : JSON.stringify(result));
                    console.log('Data fetched successfully');
                } else {
                    console.log('No definition found for:', keyword);
                    toast({
                        title: "Không tìm thấy từ",
                        description: `Không tìm thấy từ "${keyword}" trong từ điển`,
                        variant: "destructive"
                    });
                    // Wait a moment before navigating back
                    setTimeout(() => navigate('/dictionary'), 2000);
                }
            } catch (err) {
                console.error('Error fetching word definition:', err);
                toast({
                    title: "Lỗi",
                    description: "Không thể tra cứu từ điển. Vui lòng thử lại sau.",
                    variant: "destructive"
                });
                setTimeout(() => navigate('/dictionary'), 2000);
            } finally {
                setIsLoading(false);
            }
        };

        fetchWordDefinition();
    }, [keyword, navigate, toast]);

    // Function to parse Markdown-like sections
    const parseResponse = (text: string) => {
        if (!text) return {};

        console.log('Parsing response text...');

        const sections: Record<string, string[]> = {};
        // Updated regex to match both ## 1. SECTION and ## **1. SECTION**
        const titleRegex = /##\s+\*?\*?(\d+\.\s+[\p{L}\s]+)\*?\*?/gmu;
        let currentSection = '';

        // Split by lines
        const lines = text.split('\n');

        for (let i = 0; i < lines.length; i++) {
            const line = lines[i].trim();

            if (!line) continue;

            // Check if this is a section title
            const match = titleRegex.exec(line);

            if (match) {
                currentSection = match[1].trim();
                sections[currentSection] = [];
                console.log('Found section:', currentSection);
                titleRegex.lastIndex = 0; // Reset regex
            } else if (currentSection) {
                sections[currentSection].push(line);
            }
        }

        console.log('Found', Object.keys(sections).length, 'sections');
        return sections;
    };

    // Function to render markdown-like content with proper formatting
    const renderContent = (lines: string[]) => {
        return lines.map((line, index) => {
            // Remove markdown bold formatting if present
            const cleanLine = line.replace(/\*\*(.*?)\*\*/g, '$1');

            if (line.startsWith('- ')) {
                // First level bullet point
                const content = cleanLine.substring(2);

                // Check if this is a header-like bullet point
                if (content.includes('IPA:') || content.includes('Trọng âm:') || content.includes('Cách đọc:')) {
                    return <p key={index} className="ml-6 mb-2 font-medium">{content}</p>;
                } else if (content.startsWith('Nghĩa phổ biến nhất:') ||
                    content.startsWith('Nghĩa khác:') ||
                    content.startsWith('Loại từ:') ||
                    content.startsWith('Cách dùng:') ||
                    content.startsWith('Nghĩa:')) {
                    return <p key={index} className="ml-6 mb-2 font-medium">{content}</p>;
                } else {
                    return <li key={index} className="ml-6 mb-2">{content}</li>;
                }
            } else if (line.startsWith('  - ')) {
                // Second level bullet point
                const content = cleanLine.substring(4);

                if (content.includes('Ví dụ:')) {
                    // This is an example - check if it has a translation (→)
                    const parts = content.split('→');
                    return (
                        <div key={index} className="ml-12 mb-2 italic">
                            <span>{parts[0].trim()}</span>
                            {parts.length > 1 && (
                                <span className="not-italic text-gray-700 dark:text-gray-300"> → {parts[1].trim()}</span>
                            )}
                        </div>
                    );
                } else {
                    return <li key={index} className="ml-12 mb-2">{content}</li>;
                }
            } else if (line.startsWith('    - ')) {
                // Third level bullet point
                const content = cleanLine.substring(6);

                if (content.includes('Ví dụ:')) {
                    // This is an example - check if it has a translation (→)
                    const parts = content.split('→');
                    return (
                        <div key={index} className="ml-16 mb-2 italic">
                            <span>{parts[0].trim()}</span>
                            {parts.length > 1 && (
                                <span className="not-italic text-gray-700 dark:text-gray-300"> → {parts[1].trim()}</span>
                            )}
                        </div>
                    );
                } else {
                    return <li key={index} className="ml-16 mb-2">{content}</li>;
                }
            } else if (line.startsWith('      - ')) {
                // Fourth level bullet point
                const content = cleanLine.substring(8);
                return <li key={index} className="ml-20 mb-2">{content}</li>;
            } else if (line.includes('Ví dụ:')) {
                // Example anywhere in the line
                const parts = line.split('→');
                return (
                    <div key={index} className="pl-8 mb-2 italic">
                        <span>{parts[0].trim()}</span>
                        {parts.length > 1 && (
                            <span className="not-italic text-gray-700 dark:text-gray-300"> → {parts[1].trim()}</span>
                        )}
                    </div>
                );
            } else {
                // Regular paragraph or other content
                return <p key={index} className="mb-2">{cleanLine}</p>;
            }
        });
    };

    if (isLoading) {
        return (
            <div className="min-h-screen flex flex-col bg-engace-light dark:bg-gray-900">
                <Header />
                <div className="flex-1 flex items-center justify-center">
                    <div className="text-center">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-engace-purple mx-auto mb-4"></div>
                        <p className="text-lg dark:text-white">Đang tải dữ liệu từ điển...</p>
                    </div>
                </div>
            </div>
        );
    }

    // Parse the content if it's available
    const sections = wordData ? parseResponse(wordData) : {};
    const sectionKeys = Object.keys(sections);

    return (
        <div className="min-h-screen flex flex-col bg-engace-light dark:bg-gray-900">
            <Header />
            <main className="flex-1 container max-w-screen-xl mx-auto py-4 px-4 animate-fade-in">
                <div className="max-w-3xl mx-auto bg-white dark:bg-gray-800 rounded-xl shadow-sm p-6">
                    <div className="flex justify-between mb-6">
                        <Button
                            variant="outline"
                            className="flex items-center gap-2 text-gray-600 dark:text-gray-300"
                            onClick={() => navigate('/dictionary')}
                        >
                            <ArrowLeft size={18} />
                            Quay lại
                        </Button>
                        <Button
                            variant="ghost"
                            className="rounded-full p-2 text-gray-500"
                            aria-label="Phát âm"
                            onClick={playPronunciation}
                        >
                            <Volume2 size={24} />
                        </Button>
                    </div>

                    {/* Word Title */}
                    <h1 className="text-4xl font-bold text-purple-600 uppercase mb-2">
                        {keyword}
                    </h1>
                    <Separator className="bg-gradient-to-r from-purple-500 to-blue-500 h-1 rounded-full mb-6" />

                    {sectionKeys.length > 0 ? (
                        <div className="space-y-6">
                            {sectionKeys.map((sectionKey) => (
                                <div key={sectionKey}>
                                    <h2 className="text-xl font-bold mb-4">{sectionKey}</h2>
                                    <div className="space-y-1">
                                        {renderContent(sections[sectionKey])}
                                    </div>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="text-center py-8">
                            <p className="text-gray-500 dark:text-gray-400">
                                {wordData ? (
                                    <>
                                        <div className="font-medium mb-3">Không có dữ liệu từ điển cho từ này hoặc định dạng dữ liệu không đúng</div>
                                        <div className="text-left p-4 bg-gray-100 dark:bg-gray-700 rounded-md overflow-auto max-h-96 text-xs">
                                            <pre>{wordData}</pre>
                                        </div>
                                    </>
                                ) : 'Không có dữ liệu từ điển cho từ này.'}
                            </p>
                        </div>
                    )}
                </div>
            </main>
        </div>
    );
};

export default DictionaryResult;