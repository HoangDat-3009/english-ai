import { useState, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  readingService, 
  type ReadingExercise, 
  type UserResult 
} from '@/services/exerciseService';
import { useToast } from '@/hooks/use-toast';

export const useReadingExercises = () => {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  // 1. LẤY BÀI TẬP
  const { data: exercises = [], isLoading } = useQuery({
    queryKey: ['reading-exercises-main'],
    queryFn: () => readingService.getReadingExercises(),
  });

  // 2. SINH AI
  const generateMutation = useMutation({
    mutationFn: ({
      topic,
      level,
      type
    }: {
      topic: string;
      level: 'Beginner' | 'Intermediate' | 'Advanced';
      type: 'Part 5' | 'Part 6' | 'Part 7';
    }) => readingService.generateReadingExercise(topic, level, type),
    onSuccess: (newExercise) => {
      // THÊM VÀO DANH SÁCH
      queryClient.setQueryData<ReadingExercise[]>(['reading-exercises-main'], (old) => 
        [...(old || []), newExercise]
      );
      toast({
        title: 'Exercise Generated',
        description: 'New AI-generated exercise has been created successfully.',
      });
    },
    onError: (error) => {
      toast({
        title: 'Generation Failed',
        description: error instanceof Error ? error.message : 'Failed to generate exercise',
        variant: 'destructive',
      });
    },
  });

  // 3. SUBMIT KẾT QUẢ
  const submitMutation = useMutation({
    mutationFn: ({ exerciseId, answers }: { exerciseId: number; answers: number[] }) =>
      readingService.submitReadingResult(1, exerciseId, answers), // userId=1 tạm
    onSuccess: (result: UserResult) => {
      toast({
        title: 'Results Saved',
        description: `Score: ${result.score}/${result.totalQuestions}`,
      });
    },
  });

  // 4. CALLBACK SUBMIT
  const submitResult = useCallback((
    exerciseId: number, 
    answers: number[]
  ) => {
    submitMutation.mutate({ exerciseId, answers });
  }, [submitMutation]);

  return {
    exercises,
    isLoading,
    generateExercise: generateMutation.mutate,
    isGenerating: generateMutation.isPending,
    submitResult,
  };
};