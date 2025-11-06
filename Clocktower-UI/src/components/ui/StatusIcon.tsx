export const StatusIcon = ({status}: {
    status: boolean
}) => (
    <span>
      {status ? '✔️' : '❌'}
    </span>
)