/*
 * Copyright (c) Akveo 2019. All Rights Reserved.
 * Licensed under the Single Application / Multi Application License.
 * See LICENSE_SINGLE_APP / LICENSE_MULTI_APP in the 'docs' folder for license information on type of purchased license.
 */

import { Settings } from './settings';

export interface User {
  id: number;
  role: string;
  firstName: string;
  lastName: string;
  email: string;
  name?: string;
  age: number;
  login: string;
  picture: string;
  address: Address;
  settings: Settings;
}

export interface Address {
  street: string;
  city: string;
  zipCode: string;
}

export abstract class UserData {
 
}
