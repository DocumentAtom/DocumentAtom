import { Typography } from "antd";
import { TextProps } from "antd/es/typography/Text";

const { Text } = Typography;

export type DocuAtomTextProps = TextProps & {
  weight?: number;
  fontSize?: number;
  color?: string;
};

const DocuAtomText = (props: DocuAtomTextProps) => {
  const { children, style, weight, fontSize, color, ...rest } = props;
  return (
    <Text
      style={{ fontWeight: weight, fontSize: fontSize, color: color, ...style }}
      {...rest}
    >
      {children}
    </Text>
  );
};

export default DocuAtomText;
