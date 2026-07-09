export interface Country {
  code: string;
  name: string;
}

export interface City {
  code: string;
  name: string;
  countryCode: string;
}
