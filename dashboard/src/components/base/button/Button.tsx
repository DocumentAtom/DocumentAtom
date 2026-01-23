import { Button, ButtonProps } from "antd";

interface DocuAtomButtonProps extends ButtonProps {
  weight?: number;
}

const DocuAtomButton = (props: DocuAtomButtonProps) => {
  const { weight, icon, ...rest } = props;
  return <Button {...rest} icon={icon} style={{ fontWeight: weight }} />;
};

export default DocuAtomButton;
