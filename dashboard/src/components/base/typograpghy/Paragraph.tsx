import { Typography } from "antd";
import { ParagraphProps } from "antd/es/typography/Paragraph";
import classNames from "classnames";

const { Paragraph } = Typography;

export type DocuAtomParagraphProps = ParagraphProps & {
  weight?: number;
  fontSize?: number;
  color?: string;
};

const DocuAtomParagraph = (props: DocuAtomParagraphProps) => {
  const { children, className, style, color, weight, fontSize, ...rest } =
    props;
  return (
    <Paragraph
      className={classNames(className)}
      style={{ color: color, fontWeight: weight, fontSize: fontSize, ...style }}
      {...rest}
    >
      {children}
    </Paragraph>
  );
};

export default DocuAtomParagraph;
