import { theme, ThemeConfig } from "antd";

export const DocumentAtomTheme = {
  primary: "#22AF79", //95DB7B
  primaryDark: "#2dc48a", //95DB7B
  primaryRed: "#d9383a",
  secondaryBlue: "#b1e5ff",
  secondaryYellow: "#ffe362",
  borderGray: "#C1C1C1",
  borderSecondary: "#D1D1D1",
  white: "#ffffff",
  fontFamily: '"Inter", "serif"',
  colorBgContainerDisabled: "#E9E9E9",
  textDisabled: "#bbbbbb",
  subHeadingColor: "#666666",
};

export const primaryTheme: ThemeConfig = {
  cssVar: true,
  algorithm: theme.defaultAlgorithm,
  token: {
    colorPrimary: DocumentAtomTheme.primary,
    fontFamily: DocumentAtomTheme.fontFamily,
    colorBorder: DocumentAtomTheme.borderGray,
    colorTextDisabled: DocumentAtomTheme.textDisabled,
    colorBgContainerDisabled: DocumentAtomTheme.colorBgContainerDisabled,
    colorBorderSecondary: DocumentAtomTheme.borderSecondary,
  },
  components: {
    Message: {
      fontSize: 30,
    },
    Tabs: {
      cardBg: "#F2F2F2",
      titleFontSize: 12,
    },
    Typography: {
      fontWeightStrong: 400,
    },
    Layout: {
      fontFamily: DocumentAtomTheme.fontFamily,
    },
    Menu: {},
    Card: {
      colorBorder: DocumentAtomTheme.borderGray,
    },
    Button: {
      borderRadius: 5,
      borderRadiusLG: 5,
      primaryColor: DocumentAtomTheme.white,
      defaultColor: "#333333",
      colorLink: DocumentAtomTheme.primary,
      colorLinkHover: DocumentAtomTheme.primary,
    },
    Table: {
      headerBg: "#ffffff",
      padding: 18,
      borderColor: "#d1d5db",
    },
    Collapse: {
      headerBg: DocumentAtomTheme.white,
    },
    Input: {
      borderRadiusLG: 3,
      borderRadius: 3,
      borderRadiusXS: 3,
    },
    Select: {
      borderRadiusLG: 3,
      borderRadius: 3,
      borderRadiusXS: 3,
      optionSelectedColor: DocumentAtomTheme.white,
      optionSelectedBg: DocumentAtomTheme.primary,
    },
    Pagination: {
      fontFamily: DocumentAtomTheme.fontFamily,
    },
    Form: {
      labelColor: DocumentAtomTheme.subHeadingColor,
      colorBorder: "none",
      verticalLabelPadding: 0,
    },
  },
};

export const darkTheme: ThemeConfig = {
  cssVar: true,
  algorithm: theme.darkAlgorithm,
  token: {
    colorBgBase: "#151515",
    colorPrimary: DocumentAtomTheme.primaryDark,
    fontFamily: DocumentAtomTheme.fontFamily,
    colorBorder: "#555",
    colorTextDisabled: DocumentAtomTheme.textDisabled,
    colorBgContainerDisabled: DocumentAtomTheme.colorBgContainerDisabled,
    colorBorderSecondary: "#444444",
  },
  components: {
    Message: {
      fontSize: 30,
    },
    Tabs: {
      cardBg: "#F2F2F2",
      titleFontSize: 12,
    },
    Typography: {
      fontWeightStrong: 400,
    },
    Layout: {
      fontFamily: DocumentAtomTheme.fontFamily,
    },
    Menu: {},
    Card: {
      colorBorder: "#555555 !important",
    },
    Button: {
      borderRadius: 5,
      borderRadiusLG: 5,
      primaryColor: DocumentAtomTheme.white,
      defaultColor: "#dddddd",
      colorLink: DocumentAtomTheme.primary,
      colorLinkHover: DocumentAtomTheme.primary,
      colorBgContainerDisabled: "#333333",
    },
    Table: {
      padding: 18,
      borderColor: "#d1d5db",
    },
    Collapse: {
      headerBg: DocumentAtomTheme.white,
    },
    Input: {
      borderRadiusLG: 3,
      borderRadius: 3,
      borderRadiusXS: 3,
    },
    Select: {
      borderRadiusLG: 3,
      borderRadius: 3,
      borderRadiusXS: 3,
      optionSelectedColor: DocumentAtomTheme.white,
      optionSelectedBg: DocumentAtomTheme.primary,
    },
    Pagination: {
      fontFamily: DocumentAtomTheme.fontFamily,
    },
    Form: {
      labelColor: "#aaa",
      colorBorder: "none",
      verticalLabelPadding: 0,
    },
  },
};
