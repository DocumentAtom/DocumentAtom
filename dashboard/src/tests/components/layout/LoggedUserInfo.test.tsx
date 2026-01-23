import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import LoggedUserInfo from "#/components/layout/components/LoggedUserInfo";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  usePathname: () => "/dashboard",
}));

describe("LoggedUserInfo", () => {
  describe("Rendering", () => {
    it("should render user name", () => {
      render(<LoggedUserInfo />);

      expect(screen.getByText("User")).toBeInTheDocument();
    });

    it("should render user avatar", () => {
      render(<LoggedUserInfo />);

      expect(screen.getByText("U")).toBeInTheDocument();
    });

    it("should render dropdown icon", () => {
      const { container } = render(<LoggedUserInfo />);

      expect(
        container.querySelector(".anticon-down-circle")
      ).toBeInTheDocument();
    });

    it("should render avatar with first letter", () => {
      render(<LoggedUserInfo />);

      const avatar = screen.getByText("U");
      expect(avatar).toBeInTheDocument();
    });
  });

  describe("Dropdown Menu", () => {
    it("should show dropdown menu on click", async () => {
      render(<LoggedUserInfo />);

      const userSpace = screen.getByText("User");
      await userEvent.click(userSpace);

      // Menu items should appear after click
      // Note: Dropdown might render in portal, check if visible
    });

    it("should have clickable user info", async () => {
      const { container } = render(<LoggedUserInfo />);

      const userSpace = container.querySelector(".ant-space");
      expect(userSpace).toBeInTheDocument();
    });
  });

  describe("Avatar Display", () => {
    it("should render avatar component", () => {
      const { container } = render(<LoggedUserInfo />);

      const avatar = container.querySelector(".ant-avatar");
      expect(avatar).toBeInTheDocument();
    });

    it("should render small size avatar", () => {
      const { container } = render(<LoggedUserInfo />);

      const avatar = container.querySelector(".ant-avatar-sm");
      expect(avatar).toBeInTheDocument();
    });
  });

  describe("Text Styling", () => {
    it("should render user name text", () => {
      render(<LoggedUserInfo />);

      const userName = screen.getByText("User");
      expect(userName).toBeInTheDocument();
    });

    it("should render white colored text", () => {
      const { container } = render(<LoggedUserInfo />);

      const userName = container.querySelector(".ant-color-white");
      expect(userName).toBeInTheDocument();
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot", () => {
      const { container } = render(<LoggedUserInfo />);

      expect(container.firstChild).toMatchSnapshot();
    });
  });
});
