import React, { LegacyRef } from "react";
import { Input } from "antd";
import { InputProps, InputRef } from "antd/es/input";

type DocuAtomInputProps = InputProps;

const DocuAtomInput = React.forwardRef(
  (props: DocuAtomInputProps, ref?: LegacyRef<InputRef>) => {
    const { ...rest } = props;
    return <Input ref={ref} {...rest} />;
  }
);

DocuAtomInput.displayName = "DocuAtomInput";
export default DocuAtomInput;
