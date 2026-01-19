import {Timer} from "@/components/ui";
import {RoleDistributionWidget} from "@/components/ui/RoleDistributionWidget.tsx";

export const CenterHud = () => {
    return (
        <div className="controls-center">
            <Timer/>
            <RoleDistributionWidget/>
        </div>
    );
}
