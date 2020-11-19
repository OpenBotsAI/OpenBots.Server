import { TestBed } from '@angular/core/testing';

import { ConfigValueService } from './config-value.service';

describe('ConfigValueService', () => {
  let service: ConfigValueService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ConfigValueService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
