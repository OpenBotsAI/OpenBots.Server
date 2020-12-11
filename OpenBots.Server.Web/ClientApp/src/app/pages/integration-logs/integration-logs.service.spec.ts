import { TestBed } from '@angular/core/testing';

import { IntegrationLogsService } from './integration-logs.service';

describe('IntegrationLogsService', () => {
  let service: IntegrationLogsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(IntegrationLogsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
