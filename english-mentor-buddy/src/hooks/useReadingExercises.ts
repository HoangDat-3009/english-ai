import { useToast } from '@/hooks/use-toast';
import {
    databaseStatsService,
    type ReadingExercise,
    type UserResult
} from '@/services/databaseStatsService';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';

export const useReadingExercises = () => {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  // 1. LẤY BÀI TẬP TỪ .NET API (bao gồm admin uploaded + AI generated)
  const { data: exercises = [], isLoading } = useQuery({
    queryKey: ['reading-exercises-main'],
    queryFn: () => databaseStatsService.getReadingExercises(),
  });

  // 2. SINH AI THÔNG QUA .NET API (Backend sẽ call Gemini)
  const generateMutation = useMutation({
    mutationFn: ({
      topic,
      level,
      type
    }: {
      topic: string;
      level: 'Beginner' | 'Intermediate' | 'Advanced';
      type: 'Part 5' | 'Part 6' | 'Part 7';
    }) => databaseStatsService.generateReadingExercise(topic, level, type),
    onSuccess: (newExercise: ReadingExercise) => {
      // THÊM VÀO DANH SÁCH
      queryClient.setQueryData<ReadingExercise[]>(['reading-exercises-main'], (old) => 
        old ? [...old, newExercise] : [newExercise]
      );
      toast({
        title: 'AI Exercise Generated!',
        description: `New ${newExercise.type} exercise "${newExercise.name}" created by Gemini AI.`,
      });
    },
    onError: (error) => {
      toast({
        title: 'AI Generation Failed',
        description: error instanceof Error ? error.message : 'Failed to generate exercise with AI',
        variant: 'destructive',
      });
    },
  });

  // 3. SUBMIT KẾT QUẢ VÀO .NET API
  const submitMutation = useMutation({
    mutationFn: ({ exerciseId, answers }: { exerciseId: number; answers: number[] }) =>
      databaseStatsService.submitReadingResult(1, exerciseId, answers), // userId=1 tạm
    onSuccess: (result: UserResult) => {
      toast({
        title: 'Results Saved',
        description: `Score: ${result.score}/${result.totalQuestions} - Great job!`,
      });
    },
    onError: (error) => {
      toast({
        title: 'Submit Failed',
        description: error instanceof Error ? error.message : 'Failed to save results',
        variant: 'destructive',
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