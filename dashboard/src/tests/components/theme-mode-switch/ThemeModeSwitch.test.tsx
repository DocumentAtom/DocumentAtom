import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import ThemeModeSwitch from "#/components/theme-mode-switch/ThemeModeSwitch";
import { AppContext } from "#/hooks/appHooks";
import { ThemeEnum } from "#/types/types";

describe("ThemeModeSwitch", () => {
  const mockSetTheme = jest.fn();

  const renderWithTheme = (theme: ThemeEnum) => {
    return render(
      <AppContext.Provider value={{ theme, setTheme: mockSetTheme }}>
        <ThemeModeSwitch />
      </AppContext.Provider>
    );
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe("Rendering", () => {
    it("should render theme switch", () => {
      const { container } = renderWithTheme(ThemeEnum.LIGHT);

      expect(container.querySelector("svg")).toBeInTheDocument();
    });

    it("should render in light mode by default", () => {
      renderWithTheme(ThemeEnum.LIGHT);

      const svg = document.querySelector("svg");
      expect(svg).toBeInTheDocument();
    });

    it("should render in dark mode", () => {
      renderWithTheme(ThemeEnum.DARK);

      const svg = document.querySelector("svg");
      expect(svg).toBeInTheDocument();
    });
  });

  describe("Theme Switching", () => {
    it("should switch to dark mode when clicked in light mode", async () => {
      const { container } = renderWithTheme(ThemeEnum.LIGHT);

      const switchElement = container.querySelector("svg");
      if (switchElement) {
        await userEvent.click(switchElement);
      }

      expect(mockSetTheme).toHaveBeenCalledWith(ThemeEnum.DARK);
    });

    it("should switch to light mode when clicked in dark mode", async () => {
      const { container } = renderWithTheme(ThemeEnum.DARK);

      const switchElement = container.querySelector("svg");
      if (switchElement) {
        await userEvent.click(switchElement);
      }

      expect(mockSetTheme).toHaveBeenCalledWith(ThemeEnum.LIGHT);
    });

    it("should call setTheme only once per click", async () => {
      const { container } = renderWithTheme(ThemeEnum.LIGHT);

      const switchElement = container.querySelector("svg");
      if (switchElement) {
        await userEvent.click(switchElement);
      }

      expect(mockSetTheme).toHaveBeenCalledTimes(1);
    });
  });

  describe("Snapshots", () => {
    it("should match snapshot in light mode", () => {
      const { container } = renderWithTheme(ThemeEnum.LIGHT);

      expect(container.firstChild).toMatchSnapshot();
    });

    it("should match snapshot in dark mode", () => {
      const { container } = renderWithTheme(ThemeEnum.DARK);

      expect(container.firstChild).toMatchSnapshot();
    });
  });
});
