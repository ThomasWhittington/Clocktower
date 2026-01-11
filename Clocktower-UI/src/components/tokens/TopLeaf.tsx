export const TopLeaf = ({leafCount}: { leafCount: number }) => {
        const leafUrl = `/tokenParts/leaves/leaf-top${leafCount}.png`;

        return (
            <span className="leaf" style={{backgroundImage: `url(${leafUrl})`}}/>
        )
    }
;