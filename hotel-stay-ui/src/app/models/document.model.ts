import { DocumentType } from './enums';

export interface Document {
  holderName: string;
  type: DocumentType;
  number: string;
}
