"use client";
import PageContainer from "#/components/base/pageContainer/PageContainer";
import DocuAtomTabs from "#/components/base/tabs/Tabs";
import React, { useState } from "react";
import TypeDetection from "./components/TypeDetection";
import AtomExtraction from "./components/AtomExtraction";
import styles from "./home-page.module.scss";

const HomePage = () => {
  const [selectedTab, setSelectedTab] = useState<string>("type-detection");
  const tabItems = [
    {
      key: "type-detection",
      label: "Type Detection",
    },
    {
      key: "atom-extraction",
      label: "Atom Extraction",
    },
  ];

  return (
    <PageContainer
      pageTitleClassName={styles.pageTitleContainer}
      pageTitle={
        <DocuAtomTabs
          className={styles.tabsContainer}
          items={tabItems}
          onChange={setSelectedTab}
        />
      }
    >
      {selectedTab === "type-detection" ? (
        <TypeDetection />
      ) : (
        <AtomExtraction />
      )}
    </PageContainer>
  );
};

export default HomePage;
