export interface User {
  agent?: any;
  email: string;
  forcedPasswordChange: boolean;
  isJoinOrgRequestPending: boolean;
  isUserConsentRequired: boolean;
  myOrganizations: Organization[];
  personId: string;
  refreshToken: string;
  token: string;
  userName: string;
}

export interface Organization {
  id: string;
  isAdministrator: boolean;
  name: string;
}
