import {ScriptSelect} from "@/types";
import {useFileUpload, useSetScript} from "@/hooks";

export const ScriptSelector = () => {
    const setScript = useSetScript();
    const {fileInputRef, triggerFileInput, handleFileUpload} = useFileUpload({
        onFileLoad: (content) => setScript(ScriptSelect.Custom, content)
    });
    return (
        <div className="script-selector">
            <button onClick={() => setScript(ScriptSelect.TroubleBrewing)} className="btn-outline">
                Trouble Brewing
            </button>
            <button onClick={() => setScript(ScriptSelect.SectsAndViolets)} className="btn-outline">
                Sects and Violets
            </button>
            <button onClick={() => setScript(ScriptSelect.BadMoonRising)} className="btn-outline">
                Bad Moon Rising
            </button>
            <button onClick={triggerFileInput} className="btn-outline">
                Custom
            </button>
            <input ref={fileInputRef} type="file" accept=".json" onChange={handleFileUpload} style={{display: 'none'}}/>

        </div>
    );
}

