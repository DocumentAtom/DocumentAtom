import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import DocuAtomDropdown from "#/components/base/dropdown/Dropdown";
import { MenuProps } from "antd";

describe("DocuAtomDropdown", () => {
  const items: MenuProps["items"] = [
    { key: "1", label: "Menu Item 1" },
    { key: "2", label: "Menu Item 2" },
    { key: "3", label: "Menu Item 3" },
  ];

  it("should render children", () => {
    render(
      <DocuAtomDropdown menu={{ items }}>
        <button>Click me</button>
      </DocuAtomDropdown>
    );

    expect(
      screen.getByRole("button", { name: "Click me" })
    ).toBeInTheDocument();
  });

  it("should show menu on click", async () => {
    render(
      <DocuAtomDropdown menu={{ items }} trigger={["click"]}>
        <button>Click me</button>
      </DocuAtomDropdown>
    );

    const button = screen.getByRole("button");
    await userEvent.click(button);

    expect(screen.getByText("Menu Item 1")).toBeInTheDocument();
  });

  it("should match snapshot", () => {
    const { container } = render(
      <DocuAtomDropdown menu={{ items }}>
        <button>Dropdown</button>
      </DocuAtomDropdown>
    );

    expect(container.firstChild).toMatchSnapshot();
  });
});
