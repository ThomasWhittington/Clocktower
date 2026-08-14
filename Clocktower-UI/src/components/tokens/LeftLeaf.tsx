export const LeftLeaf = () => {
    const leftLeaf = `${import.meta.env.BASE_URL}tokenParts/leaves/leaf-left.png`;

    return (
        <span className="leaf" style={{backgroundImage: `url(${leftLeaf})`}}/>
    );
};