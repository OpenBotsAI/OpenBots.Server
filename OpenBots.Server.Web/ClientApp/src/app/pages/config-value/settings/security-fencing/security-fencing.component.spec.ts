import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { SecurityFencingComponent } from './security-fencing.component';

describe('SecurityFencingComponent', () => {
  let component: SecurityFencingComponent;
  let fixture: ComponentFixture<SecurityFencingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [SecurityFencingComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SecurityFencingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
