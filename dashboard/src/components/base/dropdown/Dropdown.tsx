import { Dropdown, DropDownProps } from "antd";

const DocuAtomDropdown = (props: DropDownProps) => {
  const { children, ...rest } = props;
  return <Dropdown {...rest}>{children}</Dropdown>;
};

export default DocuAtomDropdown;
