import { useState, useEffect } from "react";

export interface FlashcardGroup {
  id: string;
  name: string;
  type: string;
  created_at: string;
  flashcardCount: number;
}

export interface FlashcardWord {
  id: string;
  english_word: string;
  vietnamese_meaning: string;
  example_sentence_en: string;
  example_sentence_vi: string;
  phonetic: string;
}

const API_BASE_URL = "https://trienkhaidb.onrender.com/api";

export const useFlashcardGroups = () => {
  const [groups, setGroups] = useState<FlashcardGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchGroups = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${API_BASE_URL}/Flashcards`);
        if (!response.ok) {
          throw new Error("Không thể tải danh sách nhóm từ vựng");
        }
        const data = await response.json();
        setGroups(data);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Đã xảy ra lỗi");
      } finally {
        setLoading(false);
      }
    };

    fetchGroups();
  }, []);

  return { groups, loading, error };
};

export const useFlashcardWords = (groupId: string | null) => {
  const [words, setWords] = useState<FlashcardWord[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!groupId) {
      setLoading(false);
      return;
    }

    const fetchWords = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${API_BASE_URL}/Flashcards/by-group/${groupId}`);
        if (!response.ok) {
          throw new Error("Không thể tải từ vựng");
        }
        const data = await response.json();
        setWords(data);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Đã xảy ra lỗi");
      } finally {
        setLoading(false);
      }
    };

    fetchWords();
  }, [groupId]);

  return { words, loading, error };
};
