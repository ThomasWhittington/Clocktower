import leaf1 from '#/tokenParts/leaves/leaf-top1.png';
import leaf2 from '#/tokenParts/leaves/leaf-top2.png';
import leaf3 from '#/tokenParts/leaves/leaf-top3.png';
import leaf4 from '#/tokenParts/leaves/leaf-top4.png';
import leaf5 from '#/tokenParts/leaves/leaf-top5.png';

export const TopLeaf = ({leafCount}: { leafCount: number }) => {
    leafCount = Math.min(leafCount, 5);
    const leafMap: Record<number, string> = {
        1: leaf1,
        2: leaf2,
        3: leaf3,
        4: leaf4,
        5: leaf5,
    };

    return (
        <span className="leaf" style={{backgroundImage: `url(${leafMap[leafCount]})`}}/>
    )
};