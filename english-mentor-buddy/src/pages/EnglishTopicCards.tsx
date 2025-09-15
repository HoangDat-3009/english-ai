import { useState, useRef, useEffect } from "react";
import { textToSpeech, speechToText } from "@/services/elevenLabsService";
import { Card, CardContent } from "@/components/ui/card";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";
import { MessageCircle, Send, Book, Coffee, Briefcase, Plane, Users, Heart, Globe, GraduationCap, ShoppingCart, Utensils, Mic, MicOff, Volume2, VolumeX } from "lucide-react";
import MainLayout from "@/layouts/MainLayout";

interface Topic {
    id: string;
    title: string;
    description: string;
    icon: React.ComponentType<{ className?: string }>;
    color: string;
    prompt: string;
}

interface Message {
    id: string;
    content: string;
    sender: 'user' | 'ai';
    timestamp: Date;
}

const topics: Topic[] = [
    {
        id: "business",
        title: "Business English",
        description: "Professional conversations, meetings, presentations",
        icon: Briefcase,
        color: "from-blue-500 to-purple-600",
        prompt: "Let's practice business English! I can help you with professional conversations, meetings, emails, and presentations. What would you like to work on?"
    },
    {
        id: "travel",
        title: "Travel & Adventure",
        description: "Airport, hotel, directions, and travel experiences",
        icon: Plane,
        color: "from-green-500 to-blue-500",
        prompt: "Ready for some travel English? We can practice conversations at airports, hotels, restaurants, and asking for directions. Where would you like to go?"
    },
    {
        id: "daily",
        title: "Daily Life",
        description: "Everyday conversations and activities",
        icon: Coffee,
        color: "from-orange-500 to-red-500",
        prompt: "Let's chat about daily life in English! We can discuss your routine, hobbies, weather, shopping, and everyday activities. What's happening in your day?"
    },
    {
        id: "social",
        title: "Social Interactions",
        description: "Making friends, small talk, social events",
        icon: Users,
        color: "from-pink-500 to-purple-500",
        prompt: "Time to practice social English! We can work on making friends, small talk, party conversations, and social interactions. Let's be social!"
    },
    {
        id: "culture",
        title: "Culture & Traditions",
        description: "Different cultures, traditions, and customs",
        icon: Globe,
        color: "from-teal-500 to-green-500",
        prompt: "Let's explore culture and traditions in English! We can discuss different customs, holidays, food, and cultural experiences. What culture interests you?"
    },
    {
        id: "education",
        title: "Education & Learning",
        description: "School, university, courses, and studying",
        icon: GraduationCap,
        color: "from-indigo-500 to-blue-600",
        prompt: "Ready to talk about education in English? We can discuss school life, university, courses, studying tips, and academic goals. What are you learning?"
    },
    {
        id: "relationships",
        title: "Relationships",
        description: "Family, friends, love, and personal connections",
        icon: Heart,
        color: "from-red-500 to-pink-500",
        prompt: "Let's practice talking about relationships in English! We can discuss family, friends, dating, and personal connections. Tell me about someone important to you!"
    },
    {
        id: "shopping",
        title: "Shopping & Fashion",
        description: "Clothes, prices, markets, and fashion trends",
        icon: ShoppingCart,
        color: "from-purple-500 to-indigo-500",
        prompt: "Time for some shopping talk in English! We can practice conversations about clothes, prices, asking for sizes, and fashion trends. What's your style?"
    },
    {
        id: "food",
        title: "Food & Dining",
        description: "Restaurants, cooking, recipes, and food culture",
        icon: Utensils,
        color: "from-yellow-500 to-orange-500",
        prompt: "Let's talk about food in English! We can discuss restaurants, cooking, recipes, ordering food, and different cuisines. What's your favorite dish?"
    }
];

export default function EnglishTopicCards() {
    const [selectedTopic, setSelectedTopic] = useState<Topic | null>(null);
    const [messages, setMessages] = useState<Message[]>([]);
    const [inputMessage, setInputMessage] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const [isRecording, setIsRecording] = useState(false);
    const [isSpeaking, setIsSpeaking] = useState(false);
    const [isAudioEnabled, setIsAudioEnabled] = useState(true);
    const [ttsLoading, setTtsLoading] = useState(false);
    // ElevenLabs TTS thử nghiệm
    const handlePlayElevenLabsTTS = async () => {
        if (!inputMessage.trim()) return;
        setTtsLoading(true);
        try {
            const audioBlob = await textToSpeech(inputMessage);
            const audioUrl = URL.createObjectURL(audioBlob);
            const audio = new Audio(audioUrl);
            audio.onended = () => {
                URL.revokeObjectURL(audioUrl);
                setTtsLoading(false);
            };
            audio.onerror = () => {
                setTtsLoading(false);
            };
            audio.play();
        } catch (err) {
            alert("Lỗi phát âm bằng ElevenLabs: " + (err as any).message);
            setTtsLoading(false);
        }
    };

    const mediaRecorderRef = useRef<MediaRecorder | null>(null);
    const audioChunksRef = useRef<Blob[]>([]);
    const speechSynthesisRef = useRef<SpeechSynthesisUtterance | null>(null);

    // Text-to-Speech function
    const speakText = (text: string) => {
        if (!isAudioEnabled || !('speechSynthesis' in window)) return;

        // Stop any current speech
        window.speechSynthesis.cancel();

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.rate = 0.9;
        utterance.pitch = 1;
        utterance.volume = 0.8;

        // Try to use an English voice
        const voices = window.speechSynthesis.getVoices();
        const englishVoice = voices.find(voice =>
            voice.lang.startsWith('en') && voice.name.includes('Female')
        ) || voices.find(voice => voice.lang.startsWith('en'));

        if (englishVoice) {
            utterance.voice = englishVoice;
        }

        utterance.onstart = () => setIsSpeaking(true);
        utterance.onend = () => setIsSpeaking(false);
        utterance.onerror = () => setIsSpeaking(false);

        speechSynthesisRef.current = utterance;
        window.speechSynthesis.speak(utterance);
    };

    // Speech-to-Text function
    const startRecording = async () => {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            const mediaRecorder = new MediaRecorder(stream);
            mediaRecorderRef.current = mediaRecorder;
            audioChunksRef.current = [];

            mediaRecorder.ondataavailable = (event) => {
                audioChunksRef.current.push(event.data);
            };

            mediaRecorder.onstop = async () => {
                const audioBlob = new Blob(audioChunksRef.current, { type: 'audio/wav' });
                await transcribeAudio(audioBlob);
                stream.getTracks().forEach(track => track.stop());
            };

            mediaRecorder.start();
            setIsRecording(true);
        } catch (error) {
            console.error('Error accessing microphone:', error);
        }
    };

    const stopRecording = () => {
        if (mediaRecorderRef.current) {
            mediaRecorderRef.current.stop();
            setIsRecording(false);
        }
    };

    // Sử dụng ElevenLabs Speech-to-Text
    const transcribeAudio = async (audioBlob: Blob) => {
        setInputMessage("Đang nhận diện giọng nói...");
        try {
            const transcript = await speechToText(audioBlob);
            setInputMessage(transcript || "Không nhận diện được nội dung");
        } catch (err) {
            setInputMessage("Lỗi nhận diện với ElevenLabs");
            console.error(err);
        }
    };

    // Enhanced send message with TTS
    const handleSendMessageWithVoice = async () => {
        if (!inputMessage.trim()) return;

        const userMessage: Message = {
            id: Date.now().toString(),
            content: inputMessage,
            sender: "user",
            timestamp: new Date()
        };

        setMessages(prev => [...prev, userMessage]);
        setInputMessage("");
        setIsLoading(true);

        // Simulate AI response (replace with actual AI integration)
        setTimeout(() => {
            let response = `Great! I understand you want to talk about "${inputMessage}". `;

            // Generate contextual responses based on keywords
            if (inputMessage.toLowerCase().includes('business') || inputMessage.toLowerCase().includes('work')) {
                response += "In business contexts, it's important to use formal language. Try phrases like 'I would like to schedule a meeting' or 'Could we discuss the proposal?'";
            } else if (inputMessage.toLowerCase().includes('travel') || inputMessage.toLowerCase().includes('trip')) {
                response += "When traveling, useful phrases include 'Excuse me, where is...?', 'How much does this cost?', and 'Could you help me find...?'";
            } else if (inputMessage.toLowerCase().includes('food') || inputMessage.toLowerCase().includes('restaurant')) {
                response += "At restaurants, you might say 'I'd like to order...', 'Could I have the menu, please?', or 'What do you recommend?'";
            } else if (inputMessage.toLowerCase().includes('hello') || inputMessage.toLowerCase().includes('hi')) {
                response += "Hello! Nice to meet you. How are you doing today? What would you like to practice?";
            } else {
                response += "Let's practice this topic together. Can you tell me more about what specifically interests you?";
            }

            const aiResponse: Message = {
                id: (Date.now() + 1).toString(),
                content: response,
                sender: "ai",
                timestamp: new Date()
            };
            setMessages(prev => [...prev, aiResponse]);
            setIsLoading(false);

            // Speak the AI response
            if (isAudioEnabled) {
                speakText(aiResponse.content);
            }
        }, 1000);
    };

    // Auto-speak AI messages when they arrive
    useEffect(() => {
        const lastMessage = messages[messages.length - 1];
        if (lastMessage && lastMessage.sender === 'ai' && isAudioEnabled) {
            // Delay to ensure message is rendered
            setTimeout(() => speakText(lastMessage.content), 500);
        }
    }, [messages, isAudioEnabled]);

    const handleTopicClick = (topic: Topic) => {
        setSelectedTopic(topic);
        setMessages([
            {
                id: "1",
                content: topic.prompt,
                sender: "ai",
                timestamp: new Date()
            }
        ]);
    };

    const handleSendMessage = async () => {
        if (!inputMessage.trim()) return;

        const userMessage: Message = {
            id: Date.now().toString(),
            content: inputMessage,
            sender: "user",
            timestamp: new Date()
        };

        setMessages(prev => [...prev, userMessage]);
        setInputMessage("");
        setIsLoading(true);

        // Simulate AI response (replace with actual AI integration)
        setTimeout(() => {
            const aiResponse: Message = {
                id: (Date.now() + 1).toString(),
                content: `That's great! Let me help you with that. Here's my response about "${inputMessage}". In real implementation, this would be connected to an AI service for natural conversation practice.`,
                sender: "ai",
                timestamp: new Date()
            };
            setMessages(prev => [...prev, aiResponse]);
            setIsLoading(false);
        }, 1000);
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSendMessageWithVoice();
        }
    };

    return (
        <MainLayout>
            <div className="min-h-screen bg-background p-6">
                <div className="max-w-6xl mx-auto">
                    <div className="text-center mb-12">
                        <h1 className="text-4xl font-bold bg-gradient-to-r from-primary via-dictionary to-exercises bg-clip-text text-transparent mb-4">
                            English Conversation Topics
                        </h1>
                        <p className="text-xl text-muted-foreground">
                            Chọn chủ đề và bắt đầu luyện tập tiếng Anh với AI
                        </p>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {topics.map((topic) => {
                            const IconComponent = topic.icon;
                            return (
                                <Card
                                    key={topic.id}
                                    className="group cursor-pointer transition-all duration-300 hover:shadow-lg hover:scale-105 bg-card border-border/50"
                                    onClick={() => handleTopicClick(topic)}
                                >
                                    <CardContent className="p-6">
                                        <div className="flex items-center mb-4">
                                            <div className={`p-3 rounded-lg bg-gradient-to-r ${topic.color} mr-4`}>
                                                <IconComponent className="w-6 h-6 text-white" />
                                            </div>
                                            <MessageCircle className="w-5 h-5 text-primary ml-auto group-hover:animate-pulse" />
                                        </div>
                                        <h3 className="text-xl font-semibold mb-2 text-foreground">
                                            {topic.title}
                                        </h3>
                                        <p className="text-muted-foreground text-sm">
                                            {topic.description}
                                        </p>
                                    </CardContent>
                                </Card>
                            );
                        })}
                    </div>
                </div>

                <Dialog open={!!selectedTopic} onOpenChange={() => setSelectedTopic(null)}>
                    <DialogContent className="max-w-2xl h-[600px] flex flex-col bg-card border-border/50">
                        <DialogHeader>
                            <DialogTitle className="flex items-center justify-between">
                                <div className="flex items-center gap-3">
                                    {selectedTopic && (
                                        <>
                                            <div className={`p-2 rounded-lg bg-gradient-to-r ${selectedTopic.color}`}>
                                                <selectedTopic.icon className="w-5 h-5 text-white" />
                                            </div>
                                            {selectedTopic.title}
                                        </>
                                    )}
                                </div>
                                <div className="flex items-center gap-2">
                                    <Button
                                        variant="ghost"
                                        size="sm"
                                        onClick={() => setIsAudioEnabled(!isAudioEnabled)}
                                        className={`${isAudioEnabled ? 'text-primary' : 'text-muted-foreground'}`}
                                    >
                                        {isAudioEnabled ? <Volume2 className="w-4 h-4" /> : <VolumeX className="w-4 h-4" />}
                                    </Button>
                                    {isSpeaking && (
                                        <div className="flex items-center gap-1 text-primary text-xs">
                                            <div className="w-2 h-2 bg-primary rounded-full animate-bounce"></div>
                                            <span>Speaking...</span>
                                        </div>
                                    )}
                                </div>
                            </DialogTitle>
                        </DialogHeader>

                        <ScrollArea className="flex-1 pr-4">
                            <div className="space-y-4">
                                {messages.map((message) => (
                                    <div
                                        key={message.id}
                                        className={`flex ${message.sender === 'user' ? 'justify-end' : 'justify-start'}`}
                                    >
                                        <div
                                            className={`max-w-[80%] rounded-lg p-3 ${message.sender === 'user'
                                                ? 'bg-primary text-primary-foreground'
                                                : 'bg-secondary text-secondary-foreground'
                                                }`}
                                        >
                                            <p className="text-sm">{message.content}</p>
                                            <p className="text-xs opacity-70 mt-1">
                                                {message.timestamp.toLocaleTimeString()}
                                            </p>
                                        </div>
                                    </div>
                                ))}
                                {isLoading && (
                                    <div className="flex justify-start">
                                        <div className="bg-secondary text-secondary-foreground rounded-lg p-3 max-w-[80%]">
                                            <div className="flex space-x-1">
                                                <div className="w-2 h-2 bg-current rounded-full animate-bounce"></div>
                                                <div className="w-2 h-2 bg-current rounded-full animate-bounce" style={{ animationDelay: '0.1s' }}></div>
                                                <div className="w-2 h-2 bg-current rounded-full animate-bounce" style={{ animationDelay: '0.2s' }}></div>
                                            </div>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </ScrollArea>

                        <div className="flex gap-2 pt-4 border-t border-border/50">
                            <Button
                                variant={isRecording ? "destructive" : "outline"}
                                size="icon"
                                onClick={isRecording ? stopRecording : startRecording}
                                disabled={isLoading}
                                className={`${isRecording ? 'animate-pulse' : ''}`}
                            >
                                {isRecording ? <MicOff className="w-4 h-4" /> : <Mic className="w-4 h-4" />}
                            </Button>
                            <Input
                                placeholder={isRecording ? "🎤 Đang nghe..." : "Gõ hoặc nói tin nhắn của bạn..."}
                                value={inputMessage}
                                onChange={(e) => setInputMessage(e.target.value)}
                                onKeyPress={handleKeyPress}
                                className="flex-1"
                                disabled={isLoading || isRecording}
                            />
                            <Button
                                onClick={handleSendMessageWithVoice}
                                disabled={!inputMessage.trim() || isLoading || isRecording}
                                className="bg-primary hover:bg-primary/90"
                            >
                                <Send className="w-4 h-4" />
                            </Button>
                            <Button
                                onClick={handlePlayElevenLabsTTS}
                                disabled={!inputMessage.trim() || ttsLoading}
                                variant="outline"
                                title="Phát thử ElevenLabs TTS"
                            >
                                {ttsLoading ? (
                                    <span className="animate-spin">🔊</span>
                                ) : (
                                    <Volume2 className="w-4 h-4" />
                                )}
                            </Button>
                        </div>
                    </DialogContent>
                </Dialog>
            </div>
        </MainLayout>
    );
}