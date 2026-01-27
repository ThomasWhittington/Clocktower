import {BasePanel} from "@/components/ui";
import {
    type ChangeEvent,
    useEffect,
    useState
} from "react";
import {useAppStore} from "@/store";

interface PaperPanelProps {
    isOpen: boolean;
    onClose: () => void;
}

export const PaperPanel = ({isOpen, onClose}: PaperPanelProps) => {
    const gameId = useAppStore((state) => state.gameId);
    const getPaperNote = useAppStore((state) => state.getPaperNote);
    const setPaperNote = useAppStore((state) => state.setPaperNote);
    const [editorContent, setEditorContent] = useState<string>("");

    useEffect(() => {
        if (gameId) {
            const savedContent = getPaperNote(gameId);
            setEditorContent(savedContent);
        }
    }, [gameId, getPaperNote]);

    const handleChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
        const newContent = event.target.value;
        setEditorContent(newContent);
        if (gameId) {
            setPaperNote(gameId, newContent);
        }
    };

    return (
        <BasePanel title="Paper" isOpen={isOpen} onClose={onClose}>
            <textarea
                className="paper-textarea"
                value={editorContent}
                onChange={handleChange}
                placeholder="Start typing your notes..."
            />
        </BasePanel>
    );
};