import { Space, SpaceProps } from "antd";
import React from "react";

export type DocuAtomSpaceProps = SpaceProps & {};
const DocuAtomSpace = (props: DocuAtomSpaceProps) => {
  return <Space {...props} />;
};

export default DocuAtomSpace;
