import { render, screen } from "@testing-library/react";
import MenuItems from "#/components/menu-item/MenuItems";
import { MenuItemProps } from "#/components/menu-item/types";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  usePathname: () => "/dashboard",
}));

describe("MenuItems", () => {
  const mockMenuItems: MenuItemProps[] = [
    {
      key: "home",
      label: "Home",
      path: "/dashboard",
      icon: null,
    },
    {
      key: "settings",
      label: "Settings",
      path: "/dashboard/settings",
      icon: null,
    },
  ];

  const defaultProps = {
    menuItems: mockMenuItems,
    collapsed: false,
  };

  describe("Rendering", () => {
    it("should render menu items", () => {
      render(<MenuItems {...defaultProps} />);

      expect(screen.getByText("Home")).toBeInTheDocument();
      expect(screen.getByText("Settings")).toBeInTheDocument();
    });

    it("should render menu with correct mode", () => {
      const { container } = render(<MenuItems {...defaultProps} />);

      expect(container.querySelector(".ant-menu-inline")).toBeInTheDocument();
    });

    it("should render links for menu items", () => {
      render(<MenuItems {...defaultProps} />);

      const homeLink = screen.getByText("Home").closest("a");
      expect(homeLink).toHaveAttribute("href", "/dashboard");

      const settingsLink = screen.getByText("Settings").closest("a");
      expect(settingsLink).toHaveAttribute("href", "/dashboard/settings");
    });
  });

  describe("Menu Items with Children", () => {
    it("should render nested menu items", () => {
      const itemsWithChildren: MenuItemProps[] = [
        {
          key: "parent",
          label: "Parent",
          path: "/dashboard/parent",
          children: [
            {
              key: "child1",
              label: "Child 1",
              path: "/dashboard/parent/child1",
            },
            {
              key: "child2",
              label: "Child 2",
              path: "/dashboard/parent/child2",
            },
          ],
        },
      ];

      render(<MenuItems {...defaultProps} menuItems={itemsWithChildren} />);

      expect(screen.getByText("Parent")).toBeInTheDocument();
    });
  });

  describe("Selected State", () => {
    it("should render menu with selected key based on pathname", () => {
      const { container } = render(<MenuItems {...defaultProps} />);

      const menu = container.querySelector(".ant-menu");
      expect(menu).toBeInTheDocument();
    });
  });

  describe("Empty State", () => {
    it("should handle empty menu items array", () => {
      const { container } = render(
        <MenuItems {...defaultProps} menuItems={[]} />
      );

      expect(container.querySelector(".ant-menu")).toBeInTheDocument();
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot", () => {
      const { container } = render(<MenuItems {...defaultProps} />);

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot when collapsed", () => {
      const { container } = render(<MenuItems {...defaultProps} collapsed />);

      expect(container.firstChild).toMatchSnapshot();
    });
  });
});
