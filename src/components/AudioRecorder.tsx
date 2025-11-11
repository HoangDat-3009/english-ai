import { useState, useRef } from "react";
import { Button } from "@/components/ui/button";
import { Mic, Square, Play, RotateCcw } from "lucide-react";
import { toast } from "sonner";

interface AudioRecorderProps {
  onRecordingComplete: (audioBlob: Blob) => void;
  disabled?: boolean;
}

export const AudioRecorder = ({ onRecordingComplete, disabled }: AudioRecorderProps) => {
  const [isRecording, setIsRecording] = useState(false);
  const [audioBlob, setAudioBlob] = useState<Blob | null>(null);
  const [audioUrl, setAudioUrl] = useState<string | null>(null);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const audioChunksRef = useRef<Blob[]>([]);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  const startRecording = async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      
      // Use audio/wav if supported, otherwise use audio/webm
      const mimeType = MediaRecorder.isTypeSupported('audio/wav') 
        ? 'audio/wav' 
        : 'audio/webm';
      
      const mediaRecorder = new MediaRecorder(stream, { mimeType });
      mediaRecorderRef.current = mediaRecorder;
      audioChunksRef.current = [];

      mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          audioChunksRef.current.push(event.data);
        }
      };

      mediaRecorder.onstop = async () => {
        const audioBlob = new Blob(audioChunksRef.current, { type: 'audio/wav' });
        
        // Convert to WAV if needed
        const wavBlob = await convertToWav(audioBlob);
        
        setAudioBlob(wavBlob);
        const url = URL.createObjectURL(wavBlob);
        setAudioUrl(url);
        
        // Stop all tracks
        stream.getTracks().forEach(track => track.stop());
        
        // Automatically submit the recording
        onRecordingComplete(wavBlob);
      };

      mediaRecorder.start();
      setIsRecording(true);
      toast.success("Đang ghi âm...");
    } catch (error) {
      console.error("Error accessing microphone:", error);
      toast.error("Không thể truy cập microphone. Vui lòng kiểm tra quyền truy cập.");
    }
  };

  const stopRecording = () => {
    if (mediaRecorderRef.current && isRecording) {
      mediaRecorderRef.current.stop();
      setIsRecording(false);
      toast.success("Đã dừng ghi âm");
    }
  };

  const convertToWav = async (blob: Blob): Promise<Blob> => {
    // If already WAV, return as is
    if (blob.type === 'audio/wav') {
      return blob;
    }

    // Convert to WAV format
    const audioContext = new AudioContext();
    const arrayBuffer = await blob.arrayBuffer();
    const audioBuffer = await audioContext.decodeAudioData(arrayBuffer);
    
    const wav = audioBufferToWav(audioBuffer);
    return new Blob([wav], { type: 'audio/wav' });
  };

  const audioBufferToWav = (buffer: AudioBuffer): ArrayBuffer => {
    const length = buffer.length * buffer.numberOfChannels * 2 + 44;
    const arrayBuffer = new ArrayBuffer(length);
    const view = new DataView(arrayBuffer);
    const channels: Float32Array[] = [];
    let offset = 0;
    let pos = 0;

    // Write WAV header
    const setUint16 = (data: number) => {
      view.setUint16(pos, data, true);
      pos += 2;
    };

    const setUint32 = (data: number) => {
      view.setUint32(pos, data, true);
      pos += 4;
    };

    // RIFF identifier
    setUint32(0x46464952);
    // file length
    setUint32(length - 8);
    // RIFF type
    setUint32(0x45564157);
    // format chunk identifier
    setUint32(0x20746d66);
    // format chunk length
    setUint32(16);
    // sample format (raw)
    setUint16(1);
    // channel count
    setUint16(buffer.numberOfChannels);
    // sample rate
    setUint32(buffer.sampleRate);
    // byte rate (sample rate * block align)
    setUint32(buffer.sampleRate * 2 * buffer.numberOfChannels);
    // block align (channel count * bytes per sample)
    setUint16(buffer.numberOfChannels * 2);
    // bits per sample
    setUint16(16);
    // data chunk identifier
    setUint32(0x61746164);
    // data chunk length
    setUint32(length - pos - 4);

    // Write interleaved data
    for (let i = 0; i < buffer.numberOfChannels; i++) {
      channels.push(buffer.getChannelData(i));
    }

    while (pos < length) {
      for (let i = 0; i < buffer.numberOfChannels; i++) {
        let sample = Math.max(-1, Math.min(1, channels[i][offset]));
        sample = sample < 0 ? sample * 0x8000 : sample * 0x7fff;
        view.setInt16(pos, sample, true);
        pos += 2;
      }
      offset++;
    }

    return arrayBuffer;
  };

  const playAudio = () => {
    if (audioUrl && audioRef.current) {
      audioRef.current.play();
    }
  };

  const resetRecording = () => {
    setAudioBlob(null);
    setAudioUrl(null);
    if (audioUrl) {
      URL.revokeObjectURL(audioUrl);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex gap-3 justify-center">
        {!isRecording && !audioBlob && (
          <Button
            onClick={startRecording}
            disabled={disabled}
            size="lg"
            className="gap-2"
          >
            <Mic className="w-5 h-5" />
            Bắt đầu ghi âm
          </Button>
        )}

        {isRecording && (
          <Button
            onClick={stopRecording}
            size="lg"
            variant="destructive"
            className="gap-2 animate-pulse"
          >
            <Square className="w-5 h-5" />
            Dừng ghi âm
          </Button>
        )}

        {audioBlob && !isRecording && (
          <>
            <Button
              onClick={playAudio}
              size="lg"
              variant="outline"
              className="gap-2"
            >
              <Play className="w-5 h-5" />
              Phát lại
            </Button>
            <Button
              onClick={resetRecording}
              size="lg"
              variant="outline"
              className="gap-2"
            >
              <RotateCcw className="w-5 h-5" />
              Ghi lại
            </Button>
          </>
        )}
      </div>

      {audioUrl && (
        <audio ref={audioRef} src={audioUrl} className="hidden" />
      )}

      {isRecording && (
        <div className="text-center">
          <div className="inline-flex items-center gap-2 text-destructive">
            <div className="w-3 h-3 bg-destructive rounded-full animate-pulse" />
            <span className="font-medium">Đang ghi âm...</span>
          </div>
        </div>
      )}
    </div>
  );
};
