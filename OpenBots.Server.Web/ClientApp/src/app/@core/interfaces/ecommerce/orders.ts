import { Observable } from 'rxjs';
import { Country } from '../common/countries';
import { GridData } from '../common/gridData';

export interface Order {
  id: number;
  name: string;
  date: Date;
  sum: Sum;
  type: string;
  status: string;
  country: Country;
}

export interface Sum {
  value: number;
  currency: string;
}

export abstract class OrderData {
}
