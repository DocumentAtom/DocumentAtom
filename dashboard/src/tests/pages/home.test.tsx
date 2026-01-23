import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import HomePage from "#/page/home-page/HomePage";
import { renderWithRedux } from "../store/utils";

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter: () => ({
    push: jest.fn(),
    replace: jest.fn(),
    refresh: jest.fn(),
  }),
  usePathname: () => "/dashboard",
}));

describe("HomePage", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render home page with tabs", () => {
      const { container } = renderWithRedux(<HomePage />);

      expect(screen.getByText("Home")).toBeInTheDocument();
      expect(
        screen.getByRole("tab", { name: "Type Detection" })
      ).toBeInTheDocument();
      expect(
        screen.getByRole("tab", { name: "Atom Extraction" })
      ).toBeInTheDocument();

      expect(container).toMatchSnapshot();
    });

    it("should render Type Detection tab by default", () => {
      renderWithRedux(<HomePage />);

      expect(
        screen.getByText("Type Detection", { selector: "h4" })
      ).toBeInTheDocument();
      expect(
        screen.getByRole("button", { name: /select file/i })
      ).toBeInTheDocument();
    });

    it("should have submit button disabled initially", () => {
      renderWithRedux(<HomePage />);

      const submitButton = screen.getByRole("button", { name: "Submit" });
      expect(submitButton).toBeDisabled();
    });
  });

  describe("Tab Navigation", () => {
    it("should switch to Atom Extraction tab", async () => {
      renderWithRedux(<HomePage />);

      const atomExtractionTab = screen.getByRole("tab", {
        name: "Atom Extraction",
      });
      await userEvent.click(atomExtractionTab);

      await waitFor(() => {
        expect(
          screen.getByText("Atom Extraction", { selector: "h4" })
        ).toBeInTheDocument();
      });
    });
  });

  describe("Type Detection Tab", () => {
    it("should render upload component", () => {
      renderWithRedux(<HomePage />);

      expect(
        screen.getByRole("button", { name: /select file/i })
      ).toBeInTheDocument();
      expect(
        screen.getByRole("button", { name: "Submit" })
      ).toBeInTheDocument();
    });

    it("should show submit button as disabled when no file selected", () => {
      renderWithRedux(<HomePage />);

      const submitButton = screen.getByRole("button", { name: "Submit" });
      expect(submitButton).toBeDisabled();
    });
  });

  describe("Atom Extraction Tab", () => {
    it("should render file type selector", async () => {
      renderWithRedux(<HomePage />);

      await userEvent.click(
        screen.getByRole("tab", { name: "Atom Extraction" })
      );

      await waitFor(() => {
        expect(screen.getByText("File Type")).toBeInTheDocument();
        expect(screen.getByRole("combobox")).toBeInTheDocument();
      });
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot for Type Detection tab", () => {
      const { container } = renderWithRedux(<HomePage />);
      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot for Atom Extraction tab", async () => {
      const { container } = renderWithRedux(<HomePage />);

      await userEvent.click(
        screen.getByRole("tab", { name: "Atom Extraction" })
      );

      await waitFor(() => {
        expect(
          screen.getByText("Atom Extraction", { selector: "h4" })
        ).toBeInTheDocument();
      });
    });
  });
});
