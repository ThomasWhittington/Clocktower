const Spinner = ({status}: {
    status: boolean
}) => (
    <span>
      {status ? '✔️' : '❌'}
    </span>
)

export default Spinner