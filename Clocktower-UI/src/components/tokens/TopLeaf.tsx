export const TopLeaf = ({leafCount}: { leafCount: number }) => {
    const leaf1 = `${import.meta.env.BASE_URL}tokenParts/leaves/leaf-top1.png`;
    const leaf2 = `${import.meta.env.BASE_URL}tokenParts/leaves/leaf-top2.png`;
    const leaf3 = `${import.meta.env.BASE_URL}tokenParts/leaves/leaf-top3.png`;
    const leaf4 = `${import.meta.env.BASE_URL}tokenParts/leaves/leaf-top4.png`;
    const leaf5 = `${import.meta.env.BASE_URL}tokenParts/leaves/leaf-top5.png`;
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