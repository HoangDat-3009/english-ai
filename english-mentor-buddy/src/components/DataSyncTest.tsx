import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { AssignmentType } from '@/services/exerciseService';
import { getAssignmentTypeName, getSynchronizedData } from '@/services/statsService';
import React from 'react';

const DataSyncTest: React.FC = () => {
  const syncData = getSynchronizedData();
  
  return (
    <div className="container mx-auto p-6 space-y-6">
      <h1 className="text-3xl font-bold text-center">🔄 Data Synchronization Test</h1>
      
      {/* User Stats */}
      <Card>
        <CardHeader>
          <CardTitle>📊 User Statistics</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="text-center">
              <div className="text-2xl font-bold text-blue-600">{syncData.userStats.completedExercises}</div>
              <div className="text-sm text-gray-600">Completed</div>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-green-600">{syncData.userStats.currentStreak}</div>
              <div className="text-sm text-gray-600">Streak</div>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-purple-600">{syncData.userStats.averageScore}%</div>
              <div className="text-sm text-gray-600">Avg Score</div>
            </div>
            <div className="text-center">
              <div className="text-2xl font-bold text-orange-600">{Math.round(syncData.userStats.totalStudyTime / 60)}h</div>
              <div className="text-sm text-gray-600">Study Time</div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Recent Activities */}
      <Card>
        <CardHeader>
          <CardTitle>📚 Recent Activities (First 5)</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {syncData.activities.slice(0, 5).map((activity) => (
              <div key={activity.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <div>
                  <div className="font-medium">{activity.topic}</div>
                  <div className="text-sm text-gray-600">{activity.type}</div>
                  <div className="text-xs text-gray-500">{activity.date}</div>
                </div>
                <div className="text-right">
                  <Badge variant={activity.score >= 90 ? 'default' : activity.score >= 80 ? 'secondary' : 'outline'}>
                    {activity.score}%
                  </Badge>
                  {activity.assignmentType && (
                    <div className="text-xs mt-1 text-gray-500">
                      Type: {getAssignmentTypeName(activity.assignmentType)}
                    </div>
                  )}
                  <div className="text-xs text-gray-500">
                    {activity.questionsCorrect}/{activity.questionsTotal} correct
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Leaderboard Preview */}
      <Card>
        <CardHeader>
          <CardTitle>🏆 Leaderboard (Top 5)</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            {syncData.leaderboard.slice(0, 5).map((entry) => (
              <div key={entry.userId} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <div className="flex items-center gap-3">
                  <div className={`w-8 h-8 rounded-full flex items-center justify-center font-bold text-sm ${
                    entry.rank === 1 ? 'bg-yellow-500 text-white' :
                    entry.rank === 2 ? 'bg-gray-400 text-white' :
                    entry.rank === 3 ? 'bg-orange-600 text-white' :
                    'bg-gray-200 text-gray-700'
                  }`}>
                    {entry.rank}
                  </div>
                  <div>
                    <div className="font-medium">{entry.username}</div>
                    <div className="text-sm text-gray-600">
                      {entry.avatar} Last seen: {new Date(entry.lastUpdate).toLocaleDateString()}
                    </div>
                  </div>
                </div>
                <div className="text-right">
                  <div className="text-lg font-bold text-blue-600">{entry.totalScore}</div>
                  <div className="text-xs text-gray-500">{entry.exams} exams</div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Assignment Types Available */}
      <Card>
        <CardHeader>
          <CardTitle>📝 Assignment Types Available</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
            {Object.values(AssignmentType)
              .filter(type => typeof type === 'number')
              .map((type) => (
                <Badge key={type} variant="outline" className="justify-center">
                  {getAssignmentTypeName(type as AssignmentType)}
                </Badge>
              ))
            }
          </div>
        </CardContent>
      </Card>

      {/* Data Integrity Check */}
      <Card>
        <CardHeader>
          <CardTitle>✅ Data Integrity</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            <div className="flex justify-between">
              <span>Total Activities:</span>
              <Badge>{syncData.activities.length}</Badge>
            </div>
            <div className="flex justify-between">
              <span>Total Achievements:</span>
              <Badge>{syncData.achievements.length}</Badge>
            </div>
            <div className="flex justify-between">
              <span>Leaderboard Entries:</span>
              <Badge>{syncData.leaderboard.length}</Badge>
            </div>
            <div className="flex justify-between">
              <span>Week Progress Days:</span>
              <Badge>{syncData.weeklyProgress.length}</Badge>
            </div>
            <div className="flex justify-between">
              <span>Data Consistency:</span>
              <Badge variant={syncData.userStats.username ? 'default' : 'destructive'}>
                {syncData.userStats.username ? '✓ Synchronized' : '✗ Missing Data'}
              </Badge>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default DataSyncTest;