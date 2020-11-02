import { TestBed } from '@angular/core/testing';

import { EmailAccountsService } from './email-accounts.service';

describe('EmailAccountsService', () => {
  let service: EmailAccountsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EmailAccountsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
