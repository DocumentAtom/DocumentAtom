import { Input } from "antd";
import { PasswordProps } from "antd/es/input";
import React from "react";

const DocuAtomPassword = ({ ...props }: PasswordProps) => {
  return <Input.Password {...props} />;
};

export default DocuAtomPassword;
