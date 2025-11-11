import { useState, useEffect } from "react";
import { ToeicTopicResponse, VocabularyTopic } from "@/types/vocabulary";

const API_BASE = "https://dethi-40sj.onrender.com/api";

interface TopicListResponse {
  totalTopics: number;
  items: {
    topicName: string;
    totalWords: number;
    sampleWords: Array<{
      word: string;
      meaningVi: string;
      pos: string;
    }>;
  }[];
}

export const useVocabularyTopics = () => {
  const [topics, setTopics] = useState<VocabularyTopic[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTopics = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${API_BASE}/ToeicToppic`);
        
        if (!response.ok) {
          throw new Error(`Failed to fetch topics: ${response.statusText}`);
        }
        
        const data: TopicListResponse = await response.json();
        
        // Transform API response to VocabularyTopic format
        const transformedTopics: VocabularyTopic[] = data.items.map((item) => ({
          id: item.topicName,
          name: item.topicName,
          totalWords: item.totalWords,
          studyingWords: Math.floor(Math.random() * 10), // Mock data
          dueWords: Math.floor(Math.random() * 5), // Mock data
          masteredWords: 0, // Mock data
          category: item.topicName.includes("CEFR") ? "TỪ VỰNG CEFR" : "TỪ VỰNG THÔNG DỤNG"
        }));
        
        setTopics(transformedTopics);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to fetch topics");
        setTopics([]);
      } finally {
        setLoading(false);
      }
    };

    fetchTopics();
  }, []);

  return { topics, loading, error };
};

export const useVocabularyTopic = (topicName: string) => {
  const [data, setData] = useState<ToeicTopicResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTopic = async () => {
      try {
        setLoading(true);
        const encodedTopic = encodeURIComponent(topicName);
        const response = await fetch(`${API_BASE}/ToeicToppic/${encodedTopic}`);
        
        if (!response.ok) {
          throw new Error(`Failed to fetch topic: ${response.statusText}`);
        }
        
        const result = await response.json();
        setData(result);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to fetch vocabulary");
        setData(null);
      } finally {
        setLoading(false);
      }
    };

    if (topicName) {
      fetchTopic();
    }
  }, [topicName]);

  return { data, loading, error };
};
