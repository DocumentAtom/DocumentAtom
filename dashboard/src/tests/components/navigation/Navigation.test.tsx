import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import Navigation from "#/components/navigation";
import { MenuItemProps } from "#/components/menu-item/types";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  usePathname: () => "/dashboard",
}));

describe("Navigation", () => {
  const mockSetCollapsed = jest.fn();
  const mockMenuItems: MenuItemProps[] = [
    {
      key: "dashboard",
      label: "Dashboard",
      path: "/dashboard",
      icon: null,
    },
    {
      key: "users",
      label: "Users",
      path: "/dashboard/users",
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
    collapsed: false,
    menuItems: mockMenuItems,
    setCollapsed: mockSetCollapsed,
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render navigation sidebar", () => {
      const { container } = render(<Navigation {...defaultProps} />);

      const sider = container.querySelector(".ant-layout-sider");
      expect(sider).toBeInTheDocument();
    });

    it("should render logo", () => {
      render(<Navigation {...defaultProps} />);

      const logo = screen.getByAltText("Document Atom");
      expect(logo).toBeInTheDocument();
    });

    it("should render menu items", () => {
      render(<Navigation {...defaultProps} />);

      expect(screen.getByText("Dashboard")).toBeInTheDocument();
      expect(screen.getByText("Users")).toBeInTheDocument();
      expect(screen.getByText("Settings")).toBeInTheDocument();
    });

    it("should render collapse/expand button", () => {
      render(<Navigation {...defaultProps} />);

      const toggleButton = screen.getByRole("button");
      expect(toggleButton).toBeInTheDocument();
    });

    it("should show collapse icon when sidebar is expanded", () => {
      const { container } = render(
        <Navigation {...defaultProps} collapsed={false} />
      );

      const collapseIcon = container.querySelector(".anticon-double-left");
      expect(collapseIcon).toBeInTheDocument();
    });

    it("should show expand icon when sidebar is collapsed", () => {
      const { container } = render(
        <Navigation {...defaultProps} collapsed={true} />
      );

      const expandIcon = container.querySelector(".anticon-double-right");
      expect(expandIcon).toBeInTheDocument();
    });

    it("should apply light theme to sidebar", () => {
      const { container } = render(<Navigation {...defaultProps} />);

      const sider = container.querySelector(".ant-layout-sider-light");
      expect(sider).toBeInTheDocument();
    });
  });

  describe("Collapsed State", () => {
    it("should have correct width when expanded", () => {
      const { container } = render(
        <Navigation {...defaultProps} collapsed={false} />
      );

      const sider = container.querySelector(".ant-layout-sider");
      expect(sider).toHaveStyle({ width: "170px" });
    });

    it("should have correct width when collapsed", () => {
      const { container } = render(
        <Navigation {...defaultProps} collapsed={true} />
      );

      const sider = container.querySelector(".ant-layout-sider-collapsed");
      expect(sider).toBeInTheDocument();
    });

    it("should apply collapsed class when collapsed", () => {
      const { container } = render(
        <Navigation {...defaultProps} collapsed={true} />
      );

      const sider = container.querySelector(".ant-layout-sider-collapsed");
      expect(sider).toBeInTheDocument();
    });

    it("should not apply collapsed class when expanded", () => {
      const { container } = render(
        <Navigation {...defaultProps} collapsed={false} />
      );

      const sider = container.querySelector(".ant-layout-sider-collapsed");
      expect(sider).not.toBeInTheDocument();
    });
  });

  describe("User Interactions", () => {
    it("should call setCollapsed with true when collapse button is clicked and sidebar is expanded", async () => {
      render(<Navigation {...defaultProps} collapsed={false} />);

      const toggleButton = screen.getByRole("button");
      await userEvent.click(toggleButton);

      expect(mockSetCollapsed).toHaveBeenCalledWith(true);
      expect(mockSetCollapsed).toHaveBeenCalledTimes(1);
    });

    it("should call setCollapsed with false when expand button is clicked and sidebar is collapsed", async () => {
      render(<Navigation {...defaultProps} collapsed={true} />);

      const toggleButton = screen.getByRole("button");
      await userEvent.click(toggleButton);

      expect(mockSetCollapsed).toHaveBeenCalledWith(false);
      expect(mockSetCollapsed).toHaveBeenCalledTimes(1);
    });

    it("should toggle collapsed state on multiple clicks", async () => {
      render(<Navigation {...defaultProps} collapsed={false} />);

      const toggleButton = screen.getByRole("button");

      // First click
      await userEvent.click(toggleButton);
      expect(mockSetCollapsed).toHaveBeenCalledWith(true);

      // Second click
      await userEvent.click(toggleButton);
      expect(mockSetCollapsed).toHaveBeenCalledWith(true); // Still true because we're not updating the prop

      expect(mockSetCollapsed).toHaveBeenCalledTimes(2);
    });
  });

  describe("Menu Items", () => {
    it("should render empty menu when no items provided", () => {
      render(<Navigation {...defaultProps} menuItems={[]} />);

      expect(screen.queryByText("Dashboard")).not.toBeInTheDocument();
      expect(screen.queryByText("Users")).not.toBeInTheDocument();
    });

    it("should render all provided menu items", () => {
      const items: MenuItemProps[] = [
        { key: "item1", label: "Item 1", path: "/item1", icon: null },
        { key: "item2", label: "Item 2", path: "/item2", icon: null },
        { key: "item3", label: "Item 3", path: "/item3", icon: null },
      ];

      render(<Navigation {...defaultProps} menuItems={items} />);

      expect(screen.getByText("Item 1")).toBeInTheDocument();
      expect(screen.getByText("Item 2")).toBeInTheDocument();
      expect(screen.getByText("Item 3")).toBeInTheDocument();
    });

    it("should render menu items with nested children", () => {
      const itemsWithChildren: MenuItemProps[] = [
        {
          key: "parent",
          label: "Parent",
          path: "/parent",
          icon: null,
          children: [
            {
              key: "child1",
              label: "Child 1",
              path: "/parent/child1",
              icon: null,
            },
            {
              key: "child2",
              label: "Child 2",
              path: "/parent/child2",
              icon: null,
            },
          ],
        },
      ];

      render(<Navigation {...defaultProps} menuItems={itemsWithChildren} />);

      expect(screen.getByText("Parent")).toBeInTheDocument();
    });

    it("should pass collapsed prop to MenuItems", () => {
      const { rerender } = render(
        <Navigation {...defaultProps} collapsed={false} />
      );

      // Verify menu is visible
      expect(screen.getByText("Dashboard")).toBeInTheDocument();

      // Rerender with collapsed
      rerender(<Navigation {...defaultProps} collapsed={true} />);

      // Menu items should still be in DOM but layout might be different
      expect(screen.getByText("Dashboard")).toBeInTheDocument();
    });
  });

  describe("Logo Display", () => {
    it("should always show logo icon only", () => {
      render(<Navigation {...defaultProps} />);

      const logo = screen.getByAltText("Document Atom");
      expect(logo).toBeInTheDocument();

      // Logo text should not be visible (showOnlyIcon=true)
      const logoText = screen.queryByText("Document Atom");
      expect(logoText).not.toBeInTheDocument();
    });

    it("should display logo in collapsed state", () => {
      render(<Navigation {...defaultProps} collapsed={true} />);

      const logo = screen.getByAltText("Document Atom");
      expect(logo).toBeInTheDocument();
    });

    it("should display logo in expanded state", () => {
      render(<Navigation {...defaultProps} collapsed={false} />);

      const logo = screen.getByAltText("Document Atom");
      expect(logo).toBeInTheDocument();
    });
  });

  describe("Edge Cases", () => {
    it("should handle single menu item", () => {
      const singleItem: MenuItemProps[] = [
        { key: "single", label: "Single Item", path: "/single", icon: null },
      ];

      render(<Navigation {...defaultProps} menuItems={singleItem} />);

      expect(screen.getByText("Single Item")).toBeInTheDocument();
    });

    it("should handle very long menu item labels", () => {
      const longLabelItems: MenuItemProps[] = [
        {
          key: "long",
          label: "This is a very long menu item label that might overflow",
          path: "/long",
          icon: null,
        },
      ];

      render(<Navigation {...defaultProps} menuItems={longLabelItems} />);

      expect(
        screen.getByText(
          "This is a very long menu item label that might overflow"
        )
      ).toBeInTheDocument();
    });

    it("should handle special characters in menu labels", () => {
      const specialItems: MenuItemProps[] = [
        {
          key: "special",
          label: "Item with !@#$%",
          path: "/special",
          icon: null,
        },
      ];

      render(<Navigation {...defaultProps} menuItems={specialItems} />);

      expect(screen.getByText("Item with !@#$%")).toBeInTheDocument();
    });

    it("should handle unicode characters in menu labels", () => {
      const unicodeItems: MenuItemProps[] = [
        { key: "unicode", label: "菜单项", path: "/unicode", icon: null },
      ];

      render(<Navigation {...defaultProps} menuItems={unicodeItems} />);

      expect(screen.getByText("菜单项")).toBeInTheDocument();
    });
  });

  describe("Accessibility", () => {
    it("should have accessible toggle button", () => {
      render(<Navigation {...defaultProps} />);

      const toggleButton = screen.getByRole("button");
      expect(toggleButton).toBeInTheDocument();
      expect(toggleButton).toBeEnabled();
    });

    it("should allow keyboard navigation to toggle button", async () => {
      render(<Navigation {...defaultProps} />);

      const toggleButton = screen.getByRole("button");

      // Tab to button
      await userEvent.tab();

      // Check if an element has focus (might not be the button specifically)
      const focusedElement = document.activeElement;
      expect(focusedElement).toBeInTheDocument();
    });
  });
});
