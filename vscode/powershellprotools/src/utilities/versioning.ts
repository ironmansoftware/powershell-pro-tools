export default class Versioning {
    DOT_PATTERN = new RegExp("\\.");
    NON_DIGIT_PATTERN = new RegExp("\\D");

    compareMajor(version1 : string, version2 : string) {
        return this.compareLevels(version1, version2, 0);
    }

    compareMinor(version1 : string, version2 : string) {
        return this.compareLevels(version1, version2, 1);
    }

    compareRevision(version1 : string, version2 : string) {
        return this.compareLevels(version1, version2, 2);
    }

    compareLevels(version1 : string, version2 : string, index : number) {
        var stringLength = index + 1,
            v1 = this.normalize(version1),
            v2 = this.normalize(version2);
    
        if (v1.length > stringLength) {
            v1.length = stringLength;
        }
        if (v2.length > stringLength) {
            v2.length = stringLength;
        }
    
        return this.cmp(v1, v2);
    }

    compare(version1 : string, version2 : string) {
        return this.cmp(this.normalize(version1), this.normalize(version2));
    };

    normalize(version : string) : string[] {
        var trimmed = version ? version.replace(/^\s*(\S*(\s+\S+)*)\s*$/, "$1") : '',
            pieces = trimmed.split(this.DOT_PATTERN),
            partsLength,
            parts = [],
            value,
            piece,
            num,
            i;
    
        for (i = 0; i < pieces.length; i += 1) {
            piece = pieces[i].replace(this.NON_DIGIT_PATTERN, '');
            num = parseInt(piece, 10);
    
            if (isNaN(num)) {
                num = 0;
            }
            parts.push(num);
        }
        partsLength = parts.length;
        for (i = partsLength - 1; i >= 0; i -= 1) {
            value = parts[i];
            if (value === 0) {
                parts.length -= 1;
            } else {
                break;
            }
        }
    
        return parts;
    }

    cmp(x : string[], y : string[]) {
        var size = Math.min(x.length, y.length),
            i;
    
        for (i = 0; i < size; i += 1) {
            if (x[i] !== y[i]) {
                return x[i] < y[i] ? -1 : 1;
            }
        }
    
        if (x.length === y.length) {
            return 0;
        }
    
        return (x.length < y.length) ? -1 : 1;
    }
}
