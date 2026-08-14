export const OrangeLeaf = () => {
    const orangeLeaf = `${import.meta.env.BASE_URL}tokenParts/leaves/leaf-orange.png`;
    return (
        <span className="leaf" style={{backgroundImage: `url(${orangeLeaf})`}}/>
    )
};