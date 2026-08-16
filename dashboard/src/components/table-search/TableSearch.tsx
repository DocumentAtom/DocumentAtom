import { Input } from "antd";
import React from "react";
import { FilterDropdownProps } from "antd/es/table/interface";
import DocuAtomFlex from "../base/flex/Flex";

const TableSearch = ({
  setSelectedKeys,
  selectedKeys,
  confirm,
  placeholder = "Search",
}: FilterDropdownProps & { placeholder?: string }) => {
  const selectedValue = selectedKeys[0];
  const inputValue =
    selectedValue === undefined || selectedValue === null
      ? undefined
      : String(selectedValue);

  return (
    <DocuAtomFlex className="p">
      <Input.Search
        autoFocus
        placeholder={placeholder}
        value={inputValue}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
          setSelectedKeys(e.target.value ? [e.target.value] : []);

          if (!e.target.value) {
            confirm();
          }
        }}
        allowClear
        onSearch={() => {
          confirm();
        }}
      />
    </DocuAtomFlex>
  );
};

export default TableSearch;
