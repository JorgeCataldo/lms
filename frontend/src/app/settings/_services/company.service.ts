import { Injectable } from '@angular/core';
import { BackendService } from '@tg4/http-infrastructure/dist/src';
import { Observable, of } from 'rxjs';
import { Company } from 'src/app/models/company.model';

@Injectable()
export class SettingsCompanyService {
    constructor(private _httpService: BackendService) { }

    public createUpdateCompany(comp: Company): Observable<any> {
        return this._httpService.post('addOrMOdifyCompany', {
        'name': comp.name,
        'cnpj': comp.cnpj,
        'address': comp.address,
        'RHname': comp.RHname,
        'RHemail': comp.RHemail,
        'RHphone': comp.RHphone,
        'ContactName': comp.ContactName,
        'ContactEmail': comp.ContactEmail,
        'ContactPhone': comp.ContactPhone,
        'size': comp.size,
        'area': comp.area,
        'yearlyHirings': comp.yearlyHirings,
        });
    }
}
