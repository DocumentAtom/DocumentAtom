import { Tabs, TabsProps } from "antd";
import React from "react";
import classNames from "classnames";

const DocuAtomTabs = ({
  custom,
  className,
  ...props
}: TabsProps & { custom?: boolean; className?: string }) => {
  return (
    <Tabs
      {...props}
      className={classNames(custom && "custom-tabs", className)}
    />
  );
};

export default DocuAtomTabs;
