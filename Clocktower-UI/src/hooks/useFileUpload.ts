import {type ChangeEvent, useRef} from 'react';

interface UseFileUploadOptions {
    onFileLoad: (content: string) => void;
}

export const useFileUpload = ({onFileLoad}: UseFileUploadOptions) => {
    const fileInputRef = useRef<HTMLInputElement>(null);

    const triggerFileInput = () => {
        fileInputRef.current?.click();
    };

    const handleFileUpload = async (event: ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) return;

        try {
            const content = await file.text();
            JSON.parse(content);
            onFileLoad(content);
        } catch (error) {
            console.error('Failed to parse JSON file:', error);
            alert('Invalid JSON file. Please upload a valid JSON file.');
        }

        event.target.value = '';
    };

    return {
        fileInputRef,
        triggerFileInput,
        handleFileUpload
    };
};