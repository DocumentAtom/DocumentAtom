import { DocumentAtomSdk } from 'documentatom-sdk';
import * as fs from 'fs';

var api = new DocumentAtomSdk('http://localhost:8000/');

const detectType = async () => {
  try {
    const fileBinary = api.Helper.convertFileToBinary(`D:/test.rtf`);
    const type = await api.TypeDetection.detectType(fileBinary, new AbortController());
    console.log(type);
  } catch (error) {
    console.log(error, 'error');
  }
};

detectType();

const extractExcelAtom = async () => {
  try {
    const fileBinary = fs.readFileSync(`D:/test.xls`);
    const atoms = await api.ExtractAtom.excel(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractExcelAtom();

const extractHtmlAtom = async () => {
  try {
    const fileBinary = fs.readFileSync(`D:/test.html`);
    const atoms = await api.ExtractAtom.html(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractHtmlAtom();

const extractMarkdownAtom = async () => {
  try {
    const fileBinary = api.Helper.convertFileToBinary(`D:/test.md`);
    const atoms = await api.ExtractAtom.markdown(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractMarkdownAtom();

const extractOcrAtom = async () => {
  try {
    const fileBinary = fs.readFileSync(`D:/test.jpg`);
    const atoms = await api.ExtractAtom.ocr(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractOcrAtom();

const extractPdfAtom = async () => {
  try {
    const fileBinary = api.Helper.convertFileToBinary(`D:/test.pdf`);
    const atoms = await api.ExtractAtom.pdf(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractPdfAtom();4

const extractPngAtom = async () => {
  try {
    const fileBinary = fs.readFileSync(`D:/test.png`);
    const atoms = await api.ExtractAtom.png(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractPngAtom();

const extractPowerpointAtom = async () => {
  try {
    const fileBinary = fs.readFileSync(`D:/test.pptx`);
    const atoms = await api.ExtractAtom.powerpoint(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractPowerpointAtom();

const extractRtfAtom = async () => {
  try {
    const fileBinary = fs.readFileSync(`D:/test.rtf`);
    const atoms = await api.ExtractAtom.rtf(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractRtfAtom();

const extractTextAtom = async () => {
  try {
    const fileBinary = fs.readFileSync(`D:/test.txt`);
    const atoms = await api.ExtractAtom.text(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};
// extractTextAtom();

const extractDocxAtom = async () => {
  try {
    const fileBinary = fs.readFileSync(`D:/test.docx`);
    const atoms = await api.ExtractAtom.word(fileBinary, new AbortController());
    console.log(atoms);
  } catch (error) {
    console.log(error, 'error');
  }
};

// extractDocxAtom();
