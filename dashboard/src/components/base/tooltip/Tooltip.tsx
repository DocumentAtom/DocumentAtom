import { Tooltip, TooltipProps } from "antd";
import React from "react";

const DocuAtomTooltip = (props: TooltipProps) => {
  const { title, placement, children, ...rest } = props;
  return (
    <Tooltip title={title} placement={placement} {...rest}>
      {children}
    </Tooltip>
  );
};

export default DocuAtomTooltip;
