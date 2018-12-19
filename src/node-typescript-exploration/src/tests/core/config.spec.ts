// tslint:disable:no-unused-expression

import "chai/register-should";
import { buildConfiguration, determineEnvironment } from "core/config";

describe("Configuration Environment", function() {

    it ("should return a value", function() {
        const env = determineEnvironment();

        (typeof(env)).should.not.be.undefined;

        env
            .should.not.be.null
            .and
            .should.not.be.empty;
    });
});

describe("Testing of Mocha", function() {
    it("should just pass", function() {
        const thing = 1;
        thing.should.equal(1, "because it's 1");
    });
});