import { VocabularyWord } from "@/types/vocabulary";

export const vocabularyWords: VocabularyWord[] = [
  {
    id: "1",
    word: "separate",
    pronunciation: "/'sep.ər.ət/",
    translation: "tách ra, chia cắt, riêng biệt",
    example: "We need to separate the papers into different piles.",
    level: "medium",
    correctCount: 0,
    incorrectCount: 0,
  },
  {
    id: "2",
    word: "marriage",
    pronunciation: "/'mær.ɪdʒ/",
    translation: "hôn nhân, sự kết hôn, đám cưới",
    example: "Their marriage lasted for 25 years.",
    level: "easy",
    correctCount: 0,
    incorrectCount: 0,
  },
  {
    id: "3",
    word: "brother-in-law",
    pronunciation: "/'brʌð.ər.ɪn.lɔː/",
    translation: "anh rể, em rể, anh vợ",
    example: "My brother-in-law is visiting us this weekend.",
    level: "medium",
    correctCount: 0,
    incorrectCount: 0,
  },
  {
    id: "4",
    word: "bring up",
    pronunciation: "/'brɪŋ ʌp/",
    translation: "nuôi nấng, đề cập, nôn",
    example: "She was brought up by her grandparents.",
    level: "medium",
    correctCount: 0,
    incorrectCount: 0,
  },
  {
    id: "5",
    word: "family",
    pronunciation: "/'fæm.əl.i/",
    translation: "gia đình",
    example: "My family is very important to me.",
    level: "easy",
    correctCount: 0,
    incorrectCount: 0,
  },
  {
    id: "6",
    word: "relationship",
    pronunciation: "/rɪ'leɪ.ʃən.ʃɪp/",
    translation: "mối quan hệ",
    example: "They have a good relationship with their neighbors.",
    level: "medium",
    correctCount: 0,
    incorrectCount: 0,
  },
  {
    id: "7",
    word: "wedding",
    pronunciation: "/'wed.ɪŋ/",
    translation: "đám cưới",
    example: "The wedding will be held next Saturday.",
    level: "easy",
    correctCount: 0,
    incorrectCount: 0,
  },
  {
    id: "8",
    word: "divorce",
    pronunciation: "/dɪ'vɔːrs/",
    translation: "ly hôn",
    example: "They decided to get a divorce after 10 years.",
    level: "medium",
    correctCount: 0,
    incorrectCount: 0,
  },
];

export const getRandomWords = (count: number): VocabularyWord[] => {
  const shuffled = [...vocabularyWords].sort(() => Math.random() - 0.5);
  return shuffled.slice(0, Math.min(count, vocabularyWords.length));
};

export const calculateNextReviewDate = (
  difficulty: 'easy' | 'medium' | 'hard' | 'known'
): Date => {
  const now = new Date();
  const daysToAdd = {
    known: 30,
    easy: 7,
    medium: 3,
    hard: 1,
  };
  now.setDate(now.getDate() + daysToAdd[difficulty]);
  return now;
};
