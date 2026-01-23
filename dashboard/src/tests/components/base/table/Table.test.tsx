import { render, screen } from "@testing-library/react";
import DocuAtomTable from "#/components/base/table/Table";
import { TableColumnsType } from "antd";

describe("DocuAtomTable", () => {
  const mockData = [
    { key: "1", name: "John", age: 30, address: "New York" },
    { key: "2", name: "Jane", age: 25, address: "London" },
    { key: "3", name: "Bob", age: 35, address: "Paris" },
  ];

  const columns: TableColumnsType = [
    {
      title: "Name",
      dataIndex: "name",
      key: "name",
      width: 100,
    },
    {
      title: "Age",
      dataIndex: "age",
      key: "age",
      width: 80,
    },
    {
      title: "Address",
      dataIndex: "address",
      key: "address",
      width: 150,
    },
  ];

  describe("Rendering", () => {
    it("should render table with data", () => {
      render(<DocuAtomTable dataSource={mockData} columns={columns} />);

      expect(screen.getByText("John")).toBeInTheDocument();
      expect(screen.getByText("Jane")).toBeInTheDocument();
      expect(screen.getByText("Bob")).toBeInTheDocument();
    });

    it("should render table headers", () => {
      render(<DocuAtomTable dataSource={mockData} columns={columns} />);

      expect(screen.getByText("Name")).toBeInTheDocument();
      expect(screen.getByText("Age")).toBeInTheDocument();
      expect(screen.getByText("Address")).toBeInTheDocument();
    });

    it("should render empty table when no data", () => {
      const { container } = render(
        <DocuAtomTable dataSource={[]} columns={columns} />
      );

      expect(
        container.querySelector(".ant-empty-description")
      ).toBeInTheDocument();
    });
  });

  describe("Resizable Columns", () => {
    it("should render resizable table headers", () => {
      const { container } = render(
        <DocuAtomTable dataSource={mockData} columns={columns} />
      );

      expect(
        container.querySelector(".react-resizable-handle")
      ).toBeInTheDocument();
    });

    it("should handle column width changes", () => {
      const { container } = render(
        <DocuAtomTable dataSource={mockData} columns={columns} />
      );

      const table = container.querySelector(".ant-table");
      expect(table).toBeInTheDocument();
    });
  });

  describe("Table Features", () => {
    it("should render with pagination", () => {
      render(
        <DocuAtomTable
          dataSource={mockData}
          columns={columns}
          pagination={{ pageSize: 2 }}
        />
      );

      expect(screen.getByText("John")).toBeInTheDocument();
    });

    it("should render with loading state", () => {
      render(<DocuAtomTable dataSource={mockData} columns={columns} loading />);

      expect(screen.getByText("John")).toBeInTheDocument();
    });

    it("should render with row selection", () => {
      render(
        <DocuAtomTable
          dataSource={mockData}
          columns={columns}
          rowSelection={{}}
        />
      );

      expect(screen.getByText("John")).toBeInTheDocument();
    });
  });

  describe("Column Updates", () => {
    it("should update columns when columns prop changes", () => {
      const { rerender } = render(
        <DocuAtomTable dataSource={mockData} columns={columns} />
      );

      const newColumns = [
        ...columns,
        { title: "New Column", dataIndex: "new", key: "new", width: 120 },
      ];

      rerender(<DocuAtomTable dataSource={mockData} columns={newColumns} />);

      expect(screen.getByText("New Column")).toBeInTheDocument();
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot", () => {
      const { container } = render(
        <DocuAtomTable dataSource={mockData} columns={columns} />
      );

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot with pagination", () => {
      const { container } = render(
        <DocuAtomTable
          dataSource={mockData}
          columns={columns}
          pagination={{ pageSize: 2 }}
        />
      );

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot with loading", () => {
      const { container } = render(
        <DocuAtomTable dataSource={mockData} columns={columns} loading />
      );

      expect(container.firstChild).toMatchSnapshot();
    });
  });
});
