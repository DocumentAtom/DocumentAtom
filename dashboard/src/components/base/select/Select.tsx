import React from "react";
import { Select, SelectProps } from "antd";

export interface OptionProps {
  value: string | number;
  label: string;
}

interface DocuAtomSelectProps
  extends Omit<SelectProps<string | number | string[]>, "options"> {
  options: OptionProps[];
}

const DocuAtomSelect = (props: DocuAtomSelectProps) => {
  const { options, placeholder, style, ...restProps } = props;

  return (
    <Select style={style} placeholder={placeholder} {...restProps}>
      {options.map((option: OptionProps) => (
        <Select.Option key={option.value} value={option.value}>
          {option.label}
        </Select.Option>
      ))}
    </Select>
  );
};

export default DocuAtomSelect;
