export const RightLeaf = () => {
    const rightLeaf = `${import.meta.env.BASE_URL}tokenParts/leaves/leaf-right.png`;
    return (
        <span className="leaf" style={{backgroundImage: `url(${rightLeaf})`}}/>
    )
};