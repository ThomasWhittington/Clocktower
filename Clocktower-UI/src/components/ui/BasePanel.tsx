import {type ReactNode, useEffect, useRef} from "react";
import {CloseButton} from "@/components/ui";

interface BasePanelProps {
    isOpen: boolean;
    onClose: () => void;
    title: string;
    children: ReactNode;
    className?: string;
}

export const BasePanel = ({isOpen, onClose, title, className, children}: BasePanelProps) => {
    const panelRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if (!isOpen) return;
        const handleEscape = (e: KeyboardEvent) => {
            if (e.key === 'Escape') onClose();
        };
        document.body.style.overflow = 'hidden';
        document.addEventListener('keydown', handleEscape);

        panelRef.current?.focus();

        return () => {
            document.body.style.overflow = '';
            document.removeEventListener('keydown', handleEscape);
        };
    }, [isOpen, onClose]);

    if (!isOpen) return null;

    return (
        <div className={className}>
            <div className="panel-backdrop">
                <div ref={panelRef} className="panel-content" tabIndex={-1}>
                    <div className="panel-header">
                        <h2 id="panel-title" className="panel-title">{title}</h2>
                        <CloseButton onClick={onClose}/>
                    </div>
                    <div className="panel-body">
                        {children}
                    </div>
                </div>
            </div>
        </div>
    );
};